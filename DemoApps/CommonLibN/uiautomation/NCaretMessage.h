// ----------------------------------------------------------------------------

#pragma once

#include <Windows.h>

// ----------------------------------------------------------------------------
namespace Native
{
	namespace UIAutomation
	{
		//
		// NCaretMessage
		//
		// Used internally to pass messages between LowLevel Hook and CaretTracker Message Queue
		//
		struct NCaretMessage
		{
			HWND hwndMsg;    
			RECT* pRect;
			DWORD eventID;

			//
			// Ctor.
			//
			// HWND hwnd: handle of caret source window
			//
			// long x:
			// long y:
			// long width:
			// long height: Caret Rectangle
			//
			// dword id: Event ID. Can be one of the following values: 
			//           EVENT_OBJECT_DESTROY, EVENT_OBJECT_HIDE, EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_CREATE, EVENT_OBJECT_SHOW
			//
			NCaretMessage(HWND hwnd, long x, long y, long width, long height, DWORD id)
			{
				hwndMsg = hwnd;

				pRect = new RECT();

				pRect->left = x;
				pRect->top = y;
				pRect->right = x + width;
				pRect->bottom = y + height;			

				eventID = id;
			}

			//
			// Dtor.
			//
			~NCaretMessage()
			{
				delete pRect;
				pRect = NULL;
			}
		};
	}
}
