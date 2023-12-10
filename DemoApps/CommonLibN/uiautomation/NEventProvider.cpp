// ----------------------------------------------------------------------------
#include "pch.h"
#include "NEventProvider.h"
#include <stdio.h>


using namespace Native::UIAutomation;

// ----------------------------------------------------------------------------
// Ctor.
// Creates NEventProvider.
// ----------------------------------------------------------------------------
NEventProvider::NEventProvider(NUIAutomation* pAutomation) : _refCount(1)
{
	_currentElement = new NScrollElem();

	this->pAutomation = pAutomation;

	_unhandledEvent = false;
	
	InitializeCriticalSection( &_mutex );
}

// ----------------------------------------------------------------------------
// dtor.
// Free resources.
// ----------------------------------------------------------------------------
NEventProvider::~NEventProvider(void)
{
	if(_currentElement != NULL)
	{
		delete _currentElement;
		_currentElement = NULL;
	}
}

// ----------------------------------------------------------------------------
// AddRef.
// IUnknown Interface.
// ----------------------------------------------------------------------------
ULONG NEventProvider::AddRef()
{
	ULONG ret = InterlockedIncrement(&_refCount);
    return ret;
}

// ----------------------------------------------------------------------------
// Release.
// IUnknown Interface.
// ----------------------------------------------------------------------------
ULONG NEventProvider::Release()
{
	ULONG ret = InterlockedDecrement(&_refCount);

    if (ret == 0) 
    {
		DeleteCriticalSection( &_mutex ); 
        delete this;
        return 0;
    }

    return ret;
}

// ----------------------------------------------------------------------------
// QueryInterface.
// IUnknown Interface.
// ----------------------------------------------------------------------------
HRESULT NEventProvider::QueryInterface(REFIID riid, void** ppInterface)
{
	if (riid == __uuidof(IUnknown) || riid == __uuidof(IUIAutomationEventHandler))
	{
		*ppInterface = static_cast<IUIAutomationEventHandler*>(this);

		this->AddRef();
		return S_OK;	
	}
    else
	{
		*ppInterface = NULL;
        return E_NOINTERFACE;		
	}   	
}

// ----------------------------------------------------------------------------
// IUIAutomationHandleAutomationEventHandler
// stores NElem
// ----------------------------------------------------------------------------
HRESULT NEventProvider::HandleAutomationEvent(IUIAutomationElement *sender, EVENTID eventId)
{
	switch (eventId) 
	{
		case UIA_MenuClosedEventId:
			OutputDebugString(L"Menu closed\n");
			
			//Clear element
			EnterCriticalSection(&_mutex);

			_currentElement->Clear();

			_unhandledEvent = true;

			LeaveCriticalSection(&_mutex);

			break;

		case UIA_MenuOpenedEventId:

			OutputDebugString(L"Menu open\n");

			NScrollElem* pScrollElem;
			
			if(pAutomation->SearchSubtree(sender, &pScrollElem) && pScrollElem != NULL)
			{
				OutputDebugString(L"Scroll pattern found\n");

				//scroll elem found in subtree

				EnterCriticalSection(&_mutex);
			
				//Copy elem
				_currentElement->Clear();
							
				_currentElement->name = pScrollElem->name;
				_currentElement->bounds = pScrollElem->bounds;
				_currentElement->controlType = pScrollElem->controlType;
				_currentElement->isHorizontalScrollable = pScrollElem->isHorizontalScrollable;
				_currentElement->isVerticalScrollable = pScrollElem->isVerticalScrollable;
				_currentElement->parentIsScrollable = pScrollElem->parentIsScrollable;
				_currentElement->pScrollPattern = pScrollElem->pScrollPattern;			

				_unhandledEvent = true;

				LeaveCriticalSection(&_mutex);

				//delete pScrollElem; ??
			}								
			else
			{
				OutputDebugString(L"Scroll pattern NOT found\n");

				//Clear element
				EnterCriticalSection(&_mutex);

				_currentElement->Clear();

				_unhandledEvent = true;

				LeaveCriticalSection(&_mutex);
			}

			break;
	}

	return S_OK;
}

// ----------------------------------------------------------------------------
// GetLastFocusedElement
// returns last focused element
// ----------------------------------------------------------------------------
bool NEventProvider::GetScrollElement(NScrollElem* targetElement)
{
	bool ret = false;

	EnterCriticalSection(&_mutex);	

	if(targetElement != NULL && _currentElement != NULL)
	{		
		//copy element
		_currentElement->Clone(*targetElement);		

		ret = true;
	}

	_unhandledEvent = false;
			
	LeaveCriticalSection(&_mutex);

	return ret;
}

// ----------------------------------------------------------------------------
// Returns true if we have unhandled Events
// ----------------------------------------------------------------------------
bool NEventProvider::HasUnhandledEvents()
{
	EnterCriticalSection(&_mutex);

	bool unhandledEvents = _unhandledEvent;
			
	LeaveCriticalSection(&_mutex);

	return unhandledEvents;
}