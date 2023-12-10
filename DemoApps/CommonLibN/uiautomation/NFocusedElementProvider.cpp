// ----------------------------------------------------------------------------
#include "pch.h"
#include "NFocusedElementProvider.h"

using namespace Native::UIAutomation;

// ----------------------------------------------------------------------------
// Ctor.
// Creates NFocusedElementProvider.
// ----------------------------------------------------------------------------
NFocusedElementProvider::NFocusedElementProvider() : _refCount(1)
{
	_currentElement = NULL;
	
	InitializeCriticalSection( &_mutex );
}

// ----------------------------------------------------------------------------
// dtor.
// Free resources.
// ----------------------------------------------------------------------------
NFocusedElementProvider::~NFocusedElementProvider(void)
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
ULONG NFocusedElementProvider::AddRef()
{
	ULONG ret = InterlockedIncrement(&_refCount);
    return ret;
}

// ----------------------------------------------------------------------------
// Release.
// IUnknown Interface.
// ----------------------------------------------------------------------------
ULONG NFocusedElementProvider::Release()
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
HRESULT NFocusedElementProvider::QueryInterface(REFIID riid, void** ppInterface)
{
	if (riid == __uuidof(IUnknown) || riid == __uuidof(IUIAutomationFocusChangedEventHandler))
	{
		*ppInterface = static_cast<IUIAutomationFocusChangedEventHandler*>(this);

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
// IUIAutomationFocusChangedEventHandler
// stores NElem
// ----------------------------------------------------------------------------
HRESULT NFocusedElementProvider::HandleFocusChangedEvent(IUIAutomationElement *sender)
{
	EnterCriticalSection(&_mutex);

	if(_currentElement != NULL)
	{
		delete _currentElement;
	}

	_currentElement = new NElem();	

	sender->get_CachedName(&_currentElement->name);
	sender->get_CachedBoundingRectangle(&_currentElement->bounds);
	sender->get_CachedControlType(&_currentElement->controlType);

	LeaveCriticalSection(&_mutex);

	return S_OK;
}

// ----------------------------------------------------------------------------
// GetLastFocusedElement
// returns last focused element
// ----------------------------------------------------------------------------
bool NFocusedElementProvider::GetLastFocusedElement(NElem* targetElement)
{
	bool result;

	EnterCriticalSection(&_mutex);	

	if(targetElement == NULL || _currentElement == NULL)
	{
		result = false;
	}
	else
	{	
		//copy element
		_currentElement->Clone(*targetElement);		

		result = true;
	}
			
	LeaveCriticalSection(&_mutex);

	return result;
}

// ----------------------------------------------------------------------------