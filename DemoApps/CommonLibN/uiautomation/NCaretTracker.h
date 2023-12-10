// ----------------------------------------------------------------------------
// Description: Caret Tracker implementation based on MSAA
//
//              This class creates it's own worker thread and it's own message 
//              queue, so it's safe to use that class from an UI thread.
//
// Important:   It's not safe to instantiate multiple instances simultaneously,
//              because the Low-Level Hook can't be linked to multiple objects.
//
// ----------------------------------------------------------------------------

#pragma once

#include <map>
#include <dwmapi.h>

#ifndef NATIVE_NOAUTOLIB
    #pragma comment(lib, "Dwmapi.lib")    	
	#pragma comment(lib, "Oleacc.lib")    
#endif

// ----------------------------------------------------------------------------

namespace Native
{
	namespace UIAutomation
	{
		class NCaretTracker
		{
		public:
			//Ctor. Caret Changed Messages will be send to hwndOutput
			NCaretTracker(HWND hwndOutput, int paddingLeft, int paddingTop, int paddingRight, int paddingBottom);
			//Dtor
			~NCaretTracker(void);

			//The output window. Can be NULL if you don't want to visualize the caret output.
			//Furthermore, if outputWindow is NULL, you won't receive MSG_CARET messages.
			void ChangeOutputWindow(HWND outputWindow);

		private:
			//On MSAA Caret Message
			void OnMSAACaretMessage(WPARAM wp, LPARAM lp);

			//On garbage collection
			void OnGarbageCollection(UINT nIDEvent);

			//Get thumbnail which is associated with a specified window. 
			//If no thumbnail is associated with the window a new thumbnail will be created.
			HTHUMBNAIL GetThumbnail(HWND hwndKey, HWND hwndRoot);

			//Remove thumbnail which is associated with a specified window
			void RemoveThumbnail(HWND wndKey);

			//Garbage Collection
			bool IsGarbageCollectorActive();
			void ActivateGarbageCollector();
			void StopGarbageCollector();
			void RestartGarbageCollector();

			void ChangeTumbnailOutputWindow(HWND hwnd);

			//Static wrapper function for worker thread. lpParameter must contain instance pointer
			static DWORD WINAPI ThreadProc(LPVOID lpParameter)
			{
				return ((NCaretTracker*)lpParameter)->ThreadProc();
			} 

			//Worker thread
			DWORD WINAPI ThreadProc();

		private:
			//low level event hook
			HWINEVENTHOOK m_hook;

			//Garbage collection timer
			UINT_PTR m_nTimerGC;

			//tumbnail output window
			HWND m_wndOutput;

			//padding of output window
			int m_paddingLeft;
			int m_paddingRight;
			int m_paddingTop;
			int m_paddingBottom;

			//HWND which is pending for destruction
			HWND m_pendingDestroy;

			//Tumbnail map
			std::map<HWND, HTHUMBNAIL> m_thumbnailMap;

			//Handle of Worker Thread
			HANDLE m_MessageQueueThread;

			//ID of worker thread
			DWORD m_IDMessageQueueThread;

			//synchronization events
			HANDLE m_hThreadStarted;
			HANDLE m_hThreadEnded;
		};
	}
}

// ----------------------------------------------------------------------------