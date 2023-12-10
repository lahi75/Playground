// ----------------------------------------------------------------------------
//
//		Important: Do not use that class from an UI Thread!
//
// ----------------------------------------------------------------------------

#pragma once

#include <UIAutomationClient.h>
#include "NElem.h"

// ----------------------------------------------------------------------------

namespace Native
{
	namespace UIAutomation
	{
		class NFocusedElementProvider :	public IUIAutomationFocusChangedEventHandler
		{
		public:
			NFocusedElementProvider();
			~NFocusedElementProvider();

			//IUnknown Interface	
			ULONG STDMETHODCALLTYPE AddRef();
			ULONG STDMETHODCALLTYPE Release();
			HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppInterface);

			//IUIAutomationFocusChangedEventHandler Interface
			HRESULT STDMETHODCALLTYPE HandleFocusChangedEvent(IUIAutomationElement *sender);

			bool GetLastFocusedElement(NElem* targetElement);

		private:
			ULONG _refCount;

			NElem* _currentElement;

			CRITICAL_SECTION	_mutex;
		};
	}
}
