// ----------------------------------------------------------------------------
#include "pch.h"
#include "NCaretTracker.h"
#include "NCaretMessage.h"
#include <Oleacc.h>

using namespace Native::UIAutomation;

// ----------------------------------------------------------------------------

//private messages
#define MSG_MSAA_CARET (WM_APP + 1)
#define MSG_KILLTHREAD (WM_APP + 2)
#define MSG_CHANGEOUTPUTWINDOW (WM_APP + 3)

//This message will be send to m_wndOutput if the caret is created or destroyed
#define MSG_CARET (WM_APP + 4)

//ID of Worker Thread
DWORD g_IDMessageQueueThread; //caution: do not create more than one instance of this class .. we can't use a member variable here because of the callback function
//low level callback
void CALLBACK WinEventProc(HWINEVENTHOOK hEvent, DWORD event, HWND hwndMsg, LONG idObject, LONG idChild, DWORD idThread, DWORD dwmsEventTime);

// 
// Ctor. 
//
// HWND hwndOutput: Content on caret location will be copied to this window. 
//                  Furthermore MSG_CARET Messages will be send to this window to inform the window when the caret has been created or destroyed.
// 
// int paddingLeft: 
// int paddingTop:
// int paddingRight:
// int paddingBottom: Padding of output window. Can be used to define an offfset between the output window and the content that will be copied to the output window. Must be greater or equal to 0. 
//
NCaretTracker::NCaretTracker(HWND hwndOutput, int paddingLeft, int paddingTop, int paddingRight, int paddingBottom)
{
	m_nTimerGC = NULL;
	m_hook = NULL;
	m_pendingDestroy = NULL;

	//check whether padding is greater or equal than 0
	if(paddingLeft < 0)
		paddingLeft = 0;

	if(paddingTop < 0)
		paddingTop = 0;

	if(paddingRight < 0)
		paddingRight = 0;

	if(paddingBottom < 0)
		paddingBottom = 0;

	//output window
	m_wndOutput = hwndOutput;
	//padding of output window
	m_paddingLeft = paddingLeft;
	m_paddingRight = paddingRight;
	m_paddingTop = paddingTop;
	m_paddingBottom = paddingBottom;

	//create synchronization events
	m_hThreadStarted = CreateEvent(NULL, TRUE, FALSE, NULL);
	m_hThreadEnded = CreateEvent(NULL, TRUE, FALSE, NULL);

	//Create Thread with own message queue
	m_MessageQueueThread = CreateThread(NULL, 0, &NCaretTracker::ThreadProc, this, 0, &g_IDMessageQueueThread);

	//wait until thread has created a message queue
	WaitForSingleObject(m_hThreadStarted, INFINITE);

	//close event handle
	CloseHandle(m_hThreadStarted);

	//save id as class mamber
	m_IDMessageQueueThread = g_IDMessageQueueThread;

	//install low level hook
	m_hook = SetWinEventHook(EVENT_OBJECT_CREATE, EVENT_OBJECT_LOCATIONCHANGE, NULL, WinEventProc, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNTHREAD);
}

//
// Dtor.
//
NCaretTracker::~NCaretTracker(void)
{	
	// unhook low level event
	UnhookWinEvent(m_hook);

	// end worker thread
	PostThreadMessage(m_IDMessageQueueThread, MSG_KILLTHREAD, 0, 0);

	//wait for thread termination
	WaitForSingleObject(m_hThreadEnded, INFINITE);
	
	//close thread handle
	CloseHandle(m_MessageQueueThread);
}

//
// ChangeOutputWindow.
//
// Change the output window. Attention: Make sure that the padding of the new window has the same padding like the old one. 
//
// HWND outputWindow: Handle of new output window
//
void NCaretTracker::ChangeOutputWindow(HWND outputWindow)
{	
	PostThreadMessage(m_IDMessageQueueThread, MSG_CHANGEOUTPUTWINDOW, (WPARAM)outputWindow, 0);
}

//
// ThreadProc
//
// Worker thread with message queue
//
DWORD NCaretTracker::ThreadProc()
{
	MSG msg;
	BOOL bRet;	

	//create message queue
	PeekMessage(&msg, NULL, WM_USER, WM_USER, PM_NOREMOVE);
		
	//Signal calling thread that message queue has been created
	SetEvent(m_hThreadStarted);
	
	while((bRet = GetMessage(&msg, NULL, 0, 0)) != 0)
	{ 
		if (bRet == -1)
		{
			// handle the error and possibly exit
		}
		else
		{
			switch(msg.message)
			{					
				case MSG_MSAA_CARET:
					//MSAA event
					OnMSAACaretMessage(msg.wParam, msg.lParam);
					break;

				case WM_TIMER:
					//Garbage collector
					OnGarbageCollection(msg.wParam);
					break;

				case MSG_CHANGEOUTPUTWINDOW:
					//change thumbnail output window
					ChangeTumbnailOutputWindow((HWND)msg.wParam);
					break;

				case MSG_KILLTHREAD:
					SetEvent(m_hThreadEnded);
					return msg.wParam;
			}
		}
	}

	SetEvent(m_hThreadEnded);
	return msg.wParam; 
}

//
// WinEventProc
//
// LowLevel MSAA Callback. Posts Messages to Worker Thread Message queue
//
void CALLBACK WinEventProc(HWINEVENTHOOK hEvent, DWORD event, HWND hwndMsg, LONG idObject, LONG idChild, DWORD idThread, DWORD dwmsEventTime)
{		
	idThread;
	hEvent;
	dwmsEventTime;

	if(idObject == OBJID_CARET)
	{
		if (event == EVENT_OBJECT_DESTROY || event == EVENT_OBJECT_HIDE)
		{			
			NCaretMessage* cp = new NCaretMessage(hwndMsg, 0, 0, 0, 0, event);

			PostThreadMessage(g_IDMessageQueueThread, MSG_MSAA_CARET, 0, (LPARAM)cp);			
		}
		else if ((event == EVENT_OBJECT_LOCATIONCHANGE) || (event == EVENT_OBJECT_CREATE) || (event == EVENT_OBJECT_SHOW))
		{
			IAccessible *pAcc = NULL;
			VARIANT varChild;			

			HRESULT hr = AccessibleObjectFromEvent(hwndMsg, idObject, idChild, &pAcc, &varChild);
			if (SUCCEEDED(hr))
			{			
				long x;
				long y;
				long width;
				long height;

				hr = pAcc->accLocation(&x, &y, &width, &height, varChild);					

				if (SUCCEEDED(hr))
				{	
					// The amount of work done inside the WinEvent hook should be kept to a minimum.
					// posting a message here to work on the bounding rect outside the hook.	

					NCaretMessage* cp = new NCaretMessage(hwndMsg, x, y, width, height, event);								
					PostThreadMessage(g_IDMessageQueueThread, MSG_MSAA_CARET, (WPARAM)0, (LPARAM)cp);					
				}
				else
				{
					NCaretMessage* cp = new NCaretMessage(hwndMsg, 0, 0, 0, 0, event);
					PostThreadMessage(g_IDMessageQueueThread, MSG_MSAA_CARET, 1, (LPARAM)cp);
				}

				pAcc->Release();
			}
			else
			{
				NCaretMessage* cp = new NCaretMessage(hwndMsg, 0, 0, 0, 0, event);
				PostThreadMessage(g_IDMessageQueueThread, MSG_MSAA_CARET, 2, (LPARAM)cp);
			}
		}
	}
}

//
// OnMSAACaretMessage
//
// Handle MSAA Caret Message by worker thread
// 
// WPARAM wp: 0 == no error
//            1 == pAcc->accLocation FAILED in LowLevel-Hook
//            2 == AccessibleObjectFromEvent FAILED in LowLevel Hook
//
//  LPARAM lp: Pointer of NCaretMessage object
//
void NCaretTracker::OnMSAACaretMessage(WPARAM wp, LPARAM lp)
{		
	if(lp != NULL)
	{
		NCaretMessage* cp = (NCaretMessage*)lp;		

		if(wp == 0) //wp == 0: no error,  wp == 1: pAcc->accLocation FAILED, wp == 2: AccessibleObjectFromEvent FAILED
		{
			if(cp->eventID == EVENT_OBJECT_DESTROY || cp->eventID == EVENT_OBJECT_HIDE )
			{
				//"caret destroyed" or "caret hide" message

				if(m_pendingDestroy == NULL)
				{
					//no window is pending for destruction -> add window to garbage collector list
					m_pendingDestroy = cp->hwndMsg;

					RestartGarbageCollector();
				}
				else
				{
					if(m_pendingDestroy == cp->hwndMsg)
					{
						//window is already pending for destruction -> restart garbage collector to extend pending time
						RestartGarbageCollector();
					}
					else
					{
						//Destroy pending thumbnail 
						RemoveThumbnail(m_pendingDestroy);							

						//add current window to garbage collector list
						m_pendingDestroy = cp->hwndMsg;						

						RestartGarbageCollector();

						//send message
						bool allowSendMessage = m_thumbnailMap.size() == 0;

						if(m_wndOutput != NULL && allowSendMessage)				
							SendMessage(m_wndOutput, MSG_CARET, 0, 0);						
					}					
				}
			}
			else if(cp->eventID == EVENT_OBJECT_LOCATIONCHANGE || cp->eventID == EVENT_OBJECT_CREATE || cp->eventID == EVENT_OBJECT_SHOW )
			{				
				//caret has been created or location of caret has been changed									

				if(m_pendingDestroy != NULL)
				{
					if(m_pendingDestroy == cp->hwndMsg)
					{
						//window on garbage collector list has been recreated. Remove window from garbage collector list
						m_pendingDestroy = NULL;
					}
					else
					{
						//Destroy pending thumbnail
						RemoveThumbnail(m_pendingDestroy);						

						//clear garbage collector list
						m_pendingDestroy = NULL;

						//send message
						bool allowSendMessage = m_thumbnailMap.size() == 0;

						if(m_wndOutput != NULL && allowSendMessage)				
							SendMessage(m_wndOutput, MSG_CARET, 0, 0);
					}
				}
				
				//stop garbage collector because g_pendingDestroy is always zero at this point
				StopGarbageCollector();				

				//get screen coordinates of caret rect
				int x = cp->pRect->left;
				int y = cp->pRect->top;
				int width = cp->pRect->right - cp->pRect->left;
				int height = cp->pRect->bottom - cp->pRect->top;				

				if((x == 0 && y == 0 && width == 0 && height == 0) || (height == 1)) //some controls return height of 1 ... we have to irgnore them
				{
					//not valid .. ignore window
				}
				else
				{
					bool allowSendMessage = m_thumbnailMap.size() == 0;

					//get root window
					HWND hwndRoot = ::GetAncestor(cp->hwndMsg, GA_ROOT);

					//Get thumnail from list or create a new thumbnail
					HTHUMBNAIL thumbnail = GetThumbnail(cp->hwndMsg, hwndRoot);
					
					if(thumbnail != NULL)
					{							
						//Transform caret rect to client coordinates
						RECT windowRect;
						::GetWindowRect(hwndRoot, &windowRect);

						cp->pRect->left = cp->pRect->left - windowRect.left;
						cp->pRect->top = cp->pRect->top - windowRect.top;
						cp->pRect->right = cp->pRect->left + width;
						cp->pRect->bottom = cp->pRect->top + height;

						//rect of output window
						RECT outputTargetRect;
						GetWindowRect(m_wndOutput, &outputTargetRect);

						int targetWidth = outputTargetRect.right - outputTargetRect.left - m_paddingLeft - m_paddingRight;
						int targetHeight = outputTargetRect.bottom - outputTargetRect.top - m_paddingTop - m_paddingBottom;

						if(targetWidth > 0 && targetHeight > 0)
						{
							// Specify the destination rectangle size
							RECT dest = { m_paddingLeft, m_paddingTop, m_paddingLeft + targetWidth, m_paddingTop + targetHeight};

							RECT source = *cp->pRect;

							//adjust rect to center
							int caretHeight = (source.bottom - source.top);

							source.left -= caretHeight * (targetWidth / targetHeight);
							source.right += caretHeight;

							source.top -= caretHeight / 2;
							source.bottom += caretHeight / 2;

							// Set the thumbnail properties for use
							DWM_THUMBNAIL_PROPERTIES dskThumbProps;
							dskThumbProps.dwFlags = DWM_TNP_SOURCECLIENTAREAONLY | DWM_TNP_VISIBLE | DWM_TNP_RECTDESTINATION | DWM_TNP_RECTSOURCE;
							dskThumbProps.fSourceClientAreaOnly = FALSE; 
							dskThumbProps.fVisible = TRUE;
							dskThumbProps.rcDestination = dest;
							dskThumbProps.rcSource = source;

							// Update the thumbnail
							DwmUpdateThumbnailProperties(thumbnail, &dskThumbProps);
						}						
					}

					if(m_wndOutput != NULL && allowSendMessage)				
					{
						SendMessage(m_wndOutput, MSG_CARET, 1, 0);
					}
				}				
			}
		}

		//clean up caret message
		delete cp;		
	}	
}

//
// GetThumbnail
//
// Get thumbnail which is associated with a specified window. 
// If no thumbnail is associated with the window a new thumbnail will be created.
//
HTHUMBNAIL NCaretTracker::GetThumbnail(HWND hwndKey, HWND hwndRoot)
{
	HTHUMBNAIL thumbnail = NULL;

	//try to find Thumbnail in list with the specified window as key
	std::map<HWND, HTHUMBNAIL>::iterator it = m_thumbnailMap.find(hwndKey);

	if(it == m_thumbnailMap.end())
	{
		//not in list .. register thumbnail
		if(DwmRegisterThumbnail(m_wndOutput, hwndRoot, &thumbnail) == S_OK)						
		{
			m_thumbnailMap.insert(std::pair<HWND,HTHUMBNAIL>(hwndKey, thumbnail));
		}
	}
	else
	{
		//get thumbnail from list
		thumbnail = it->second;
	}

	return thumbnail;
}

//
// RemoveThumbnail
//
// Remove thumbnail which is associated with a specified window
//
void NCaretTracker::RemoveThumbnail(HWND hwndKey)
{
	//try to find Thumbnail in list with the specified window as key
	std::map<HWND, HTHUMBNAIL>::iterator it = m_thumbnailMap.find(hwndKey);

	if(it == m_thumbnailMap.end())
	{
		//not in list .. do nothing
	}
	else
	{
		//Unregister thumbnail
		DwmUnregisterThumbnail(it->second);
	
		//erase from map
		m_thumbnailMap.erase(it);
	}
}

//
// ChangeTumbnailOutputWindow
//
// Change the output window. All existing thumbnails will be removed
//
void NCaretTracker::ChangeTumbnailOutputWindow(HWND hwnd)
{	
	std::map<HWND, HTHUMBNAIL>::iterator it;

	//clear all thumbnails
	for(it=m_thumbnailMap.begin(); it != m_thumbnailMap.end(); it++)
	{
		//Unregister thumbnail
		DwmUnregisterThumbnail(it->second);
	
		//erase from map
		m_thumbnailMap.erase(it);
	}

	m_pendingDestroy = NULL;

	//set new output window
	m_wndOutput = hwnd;
}


//
// OnGarbageCollection
//
// Handle Garbage Collection by worker thread
//
void NCaretTracker::OnGarbageCollection(UINT nIDEvent)
{
	if(nIDEvent == m_nTimerGC)
	{
		//Stop timer
		StopGarbageCollector();

		if(m_pendingDestroy != NULL)
		{
			//destroy pending thumbnail
			RemoveThumbnail(m_pendingDestroy);		

			//clear garbage collector list
			m_pendingDestroy = NULL;

			//send message
			bool allowSendMessage = m_thumbnailMap.size() == 0;

			if(m_wndOutput != NULL && allowSendMessage)				
				SendMessage(m_wndOutput, MSG_CARET, 0, 0);				
		}	
	}
}

//
// IsGarbageCollectorActive
//
bool NCaretTracker::IsGarbageCollectorActive()
{
	return m_nTimerGC != NULL;	
}

//
// ActivateGarbageCollector
//
void NCaretTracker::ActivateGarbageCollector()
{
	if(!IsGarbageCollectorActive())
	{
		//Start garbage collection after 100 ms
		m_nTimerGC = SetTimer(NULL, 1, 100, NULL);
	}
}

//
// StopGarbageCollector
//
void NCaretTracker::StopGarbageCollector()
{
	KillTimer(NULL, m_nTimerGC);
	m_nTimerGC = NULL;
}

//
// RestartGarbageCollector
//
void NCaretTracker::RestartGarbageCollector()
{
	StopGarbageCollector();
	ActivateGarbageCollector();
}
