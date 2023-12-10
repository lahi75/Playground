// ----------------------------------------------------------------------------
//
//		Important: Do not use that class from an UI Thread!
//
// ----------------------------------------------------------------------------

#pragma once

#include <UIAutomationClient.h>
#include "NUIAutomation.h"
#include "NScrollElem.h"

// ----------------------------------------------------------------------------

namespace Native
{
	namespace UIAutomation
	{
		class NEventProvider :	public IUIAutomationEventHandler
		{
		public:
			NEventProvider(NUIAutomation* pAutomation);
			~NEventProvider();

			//IUnknown Interface	
			ULONG STDMETHODCALLTYPE AddRef();
			ULONG STDMETHODCALLTYPE Release();
			HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppInterface);

			//IUIAutomationFocusChangedEventHandler Interface
			HRESULT STDMETHODCALLTYPE HandleAutomationEvent(IUIAutomationElement *sender, EVENTID eventId);

			bool GetScrollElement(NScrollElem* targetElement);

			bool HasUnhandledEvents();

		private:
			ULONG _refCount;

			NScrollElem* _currentElement;

			NUIAutomation* pAutomation;

			bool _unhandledEvent;

			CRITICAL_SECTION	_mutex;
		};
	}
}