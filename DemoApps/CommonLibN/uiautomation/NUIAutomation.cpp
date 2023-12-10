// ----------------------------------------------------------------------------
#include "pch.h"
#include "NUIAutomation.h"

using namespace Native::UIAutomation;

// ----------------------------------------------------------------------------
// Ctor.
// Creates IUIAutomation object and cache request.
// ----------------------------------------------------------------------------
NUIAutomation::NUIAutomation()
{
	IUIAutomation* pAutomation;		
	
	//Get IUIAutomation COM Object
	if(CoCreateInstance(__uuidof(CUIAutomation), NULL, CLSCTX_INPROC_SERVER, __uuidof(IUIAutomation), (void **)&pAutomation) == S_OK)
	{
		this->pAutomation = pAutomation;		
	}
	else
	{
		this->pAutomation = NULL;
	}

	//Create cache request
	if(pAutomation != NULL)
	{
		IUIAutomationCacheRequest *pComplexElemCache;
		if(pAutomation->CreateCacheRequest(&pComplexElemCache) == S_OK)
		{
			this->pComplexElemCache = pComplexElemCache;

			//Add Properties
			this->pComplexElemCache->AddProperty(UIA_BoundingRectanglePropertyId);
			this->pComplexElemCache->AddProperty(UIA_NamePropertyId);
			this->pComplexElemCache->AddProperty(UIA_ControlTypePropertyId);
			this->pComplexElemCache->AddProperty(UIA_ValueIsReadOnlyPropertyId);
			this->pComplexElemCache->AddProperty(UIA_ClassNamePropertyId);
			this->pComplexElemCache->AddProperty(UIA_NativeWindowHandlePropertyId);
			this->pComplexElemCache->AddProperty(UIA_ValueValuePropertyId);			

			//Add Patterns			
			this->pComplexElemCache->AddPattern(UIA_ValuePatternId);
			this->pComplexElemCache->AddPattern(UIA_ScrollItemPatternId);
			
			//Set cache options
			this->pComplexElemCache->put_TreeScope(TreeScope_Element);
			this->pComplexElemCache->put_AutomationElementMode(AutomationElementMode_Full);	
		}
		else
		{
			this->pComplexElemCache = NULL;
		}

		//Create Cache for Scroll elements
		IUIAutomationCacheRequest *pScrollElemCache;
		if(pAutomation->CreateCacheRequest(&pScrollElemCache) == S_OK)
		{
			this->pScrollElemCache = pScrollElemCache;

			//Add Properties
			this->pScrollElemCache->AddProperty(UIA_BoundingRectanglePropertyId);
			this->pScrollElemCache->AddProperty(UIA_NamePropertyId);
			this->pScrollElemCache->AddProperty(UIA_ControlTypePropertyId);
			this->pScrollElemCache->AddProperty(UIA_ScrollHorizontallyScrollablePropertyId);
			this->pScrollElemCache->AddProperty(UIA_ScrollVerticallyScrollablePropertyId);			

			//Add Patterns			
			this->pScrollElemCache->AddPattern(UIA_ScrollPatternId);
			this->pScrollElemCache->AddPattern(UIA_ScrollItemPatternId);			
			
			//Set cache options			
			this->pScrollElemCache->put_TreeScope(TreeScope_Element);
			this->pScrollElemCache->put_AutomationElementMode(AutomationElementMode_Full);	
		}
		else
		{
			this->pScrollElemCache = NULL;
		}

		//Create Property Condition
		pScrollPropertyCondition = NULL;
		
		VARIANT varCheckBox;
		varCheckBox.vt = VT_BOOL;			
		varCheckBox.boolVal = VARIANT_TRUE;

		if(pAutomation->CreatePropertyCondition( UIA_IsScrollPatternAvailablePropertyId,   varCheckBox, &pScrollPropertyCondition) != S_OK)
		{
			pScrollPropertyCondition = NULL;
		}
	}
}

bool NUIAutomation::RegisterMenuClosedEvent(IUIAutomationEventHandler* callback)
{
	if(pAutomation != NULL && pComplexElemCache != NULL)
	{
		//Get root element
		IUIAutomationElement *pRoot = NULL;
		
		if(pAutomation->GetRootElement(&pRoot) == S_OK && pRoot != NULL)
		{
			HRESULT hr1 = pAutomation->AddAutomationEventHandler(UIA_MenuClosedEventId, pRoot, TreeScope_Subtree, NULL, callback);
			HRESULT hr2 = pAutomation->AddAutomationEventHandler(UIA_MenuOpenedEventId, pRoot, TreeScope_Subtree, NULL, callback);

			return (hr1 == S_OK && hr2 == S_OK);
		}
	}

	return false;
}

// ----------------------------------------------------------------------------
// Destructor.
// Release unmanaged resources
// ----------------------------------------------------------------------------
NUIAutomation::~NUIAutomation()
{
	if(pAutomation != NULL)
	{
		pAutomation->Release();
		pAutomation = NULL;
	}

	if(pComplexElemCache != NULL)
	{
		pComplexElemCache->Release();
		pComplexElemCache = NULL;
	}

	if(pScrollElemCache != NULL)
	{
		pScrollElemCache->Release();
		pScrollElemCache = NULL;
	}
}


// ----------------------------------------------------------------------------
// SearchAtPositionUsingCache.
// Returns Element at point. Using cache for improved performence.
// ----------------------------------------------------------------------------
bool NUIAutomation::SearchAtPositionUsingCache(POINT p, NComplexElem* elem)
{
	if(this->pAutomation != NULL && elem != NULL)
	{
		IUIAutomationElement *pElement;		
							
		if(pAutomation->ElementFromPointBuildCache(p, pComplexElemCache, &pElement) == S_OK)
		{

			//Receive Properties
			VARIANT v;
			if( pElement->GetCachedPropertyValue(UIA_ValueValuePropertyId, &v) == S_OK )			
				elem->name = SysAllocString(v.bstrVal);		

			if( elem->name == NULL )
				elem->name = SysAllocString(L"");

			if(elem->name == NULL || SysStringLen(elem->name) == 0)
			{
				pElement->get_CachedName(&elem->name);					
			}			


			HRESULT hr2 = pElement->get_CachedBoundingRectangle(&elem->bounds);
			HRESULT hr3 = pElement->get_CachedControlType(&elem->controlType);
			HRESULT hr4 = pElement->get_CachedClassName(&elem->className);
			HRESULT hr5 = pElement->get_CachedNativeWindowHandle(&elem->windowHandle);																	

		

			if(elem->className == NULL)
			{
				elem->className = SysAllocString(L"");
			}

			IUIAutomationValuePattern* pValue;
			BOOL isReadOnly = TRUE;

			if(pElement->GetCachedPatternAs(UIA_ValuePatternId, __uuidof(IUIAutomationValuePattern), (void **)&pValue) != S_OK)
			{
				pValue = NULL;
			}
			

			if(pValue != NULL)
			{			
				pValue->get_CachedIsReadOnly(&isReadOnly);				

				//Release Value pattern element
				pValue->Release();
				pValue = NULL;
			}
		
			elem->hasWritableText = (isReadOnly == FALSE);

			IUIAutomationScrollItemPattern* pScrollItemPattern;

			if(pElement->GetCachedPatternAs(UIA_ScrollItemPatternId, __uuidof(IUIAutomationScrollItemPattern), (void**)&pScrollItemPattern) != S_OK)
			{
				pScrollItemPattern = NULL;
			}

			elem->parentIsScrollable = pScrollItemPattern != NULL;			

			//Free Resources
			if(pScrollItemPattern != NULL)
			{								
				pScrollItemPattern->Release();
				pScrollItemPattern = NULL;
			}
												
			//Release automation element
			pElement->Release();
			pElement = NULL;			

			return (hr2 == S_OK && hr3 == S_OK && hr4 == S_OK && hr5 == S_OK);
		}
	}
	
	return false;
}


// ----------------------------------------------------------------------------
// SearchAtPositionUsingCache.
// Returns Element at point. Using cache for improved performence.
// ----------------------------------------------------------------------------
bool NUIAutomation::SearchAtPositionUsingCache(POINT p, NScrollElem* elem)
{
	if(this->pAutomation != NULL && elem != NULL)
	{
		IUIAutomationElement *pElement;			
									
		if(pAutomation->ElementFromPointBuildCache(p, pScrollElemCache, &pElement) == S_OK)
		{
			bool success = false;
			
			//Scroll Pattern
			IUIAutomationScrollPattern* pScrollPattern;

			if(pElement->GetCachedPatternAs(UIA_ScrollPatternId, __uuidof(IUIAutomationScrollPattern), (void**)&pScrollPattern) != S_OK)
			{
				pScrollPattern = NULL;
			}

			
			if(pScrollPattern != NULL) //we are lucky .. first element is scrollable
			{
				//Receive Properties
				pElement->get_CachedName(&elem->name);
				pElement->get_CachedBoundingRectangle(&elem->bounds);
				pElement->get_CachedControlType(&elem->controlType);

				elem->pScrollPattern = pScrollPattern;
				elem->parentIsScrollable = false;
				
				pElement->Release();
				pElement = NULL;	

				return true;
			}

			//Search ancestors now !!! ... keep MAXDEPTH in mind to increase performance here

			IUIAutomationElement* pParent = NULL;
			IUIAutomationTreeWalker* pWalker = NULL;

			pAutomation->get_ControlViewWalker(&pWalker);

			if(pWalker != NULL)
			{
				const int MAXDEPTH = 5;
				int depth = 0;

				while(depth < MAXDEPTH)
				{
					//Get parent element
					if(pWalker->GetParentElementBuildCache(pElement, pScrollElemCache, &pParent) == S_OK)
					{
						if(pParent == NULL)
							break;

						//Scroll Pattern
						if(pParent->GetCachedPatternAs(UIA_ScrollPatternId, __uuidof(IUIAutomationScrollPattern), (void**)&pScrollPattern) != S_OK)
						{
							pScrollPattern = NULL;
						}

						//Scroll Item Pattern
						IUIAutomationScrollItemPattern* pScrollItemPattern;

						if(pParent->GetCachedPatternAs(UIA_ScrollItemPatternId, __uuidof(IUIAutomationScrollItemPattern), (void**)&pScrollItemPattern) != S_OK)
						{
							pScrollItemPattern = NULL;
						}					

						if(pScrollItemPattern == NULL && pScrollPattern == NULL)
						{
							//scrollItem Pattern and scrolPattern not found. Abort search.
							break;	
						}
					
						if(pScrollPattern != NULL) //yeah .. we have found the scroll pattern 
						{					
							//Receive the other Properties
							pParent->get_CachedName(&elem->name);
							pParent->get_CachedBoundingRectangle(&elem->bounds);
							pParent->get_CachedControlType(&elem->controlType);

							elem->pScrollPattern = pScrollPattern;
							elem->parentIsScrollable = pScrollItemPattern != NULL;

							BOOL verticalScrollable;
							BOOL horizontalScrollable;

							pScrollPattern->get_CachedVerticallyScrollable(&verticalScrollable);
							pScrollPattern->get_CachedHorizontallyScrollable(&horizontalScrollable);

							elem->isHorizontalScrollable = (horizontalScrollable == TRUE);
							elem->isVerticalScrollable = (verticalScrollable == TRUE);

							success = true;

							break;
						}
						
					}
					else
					{
						break;
					}

					pElement->Release();
					pElement = pParent;

					depth++;
				}

				//clean up
				if(pElement != NULL)
					pElement->Release();

				if(pWalker!=NULL)
					pWalker->Release();
			}																

			return success;	
		}
	}
	
	return false;
}

// ----------------------------------------------------------------------------
// SearchSubtree.
// ...
// ----------------------------------------------------------------------------
bool NUIAutomation::SearchSubtree(IUIAutomationElement* pRootElement, NScrollElem** ppScrollElem)
{
	*ppScrollElem = NULL;

	if(pRootElement == NULL)
		return false;

	IUIAutomationElement* pElem;

	if(pRootElement->FindFirstBuildCache(TreeScope_Subtree, pScrollPropertyCondition, pScrollElemCache, &pElem) == S_OK && pElem != NULL)
	{
		//Scroll Pattern
		IUIAutomationScrollPattern* pScrollPattern;

		if(pElem->GetCachedPatternAs(UIA_ScrollPatternId, __uuidof(IUIAutomationScrollPattern), (void**)&pScrollPattern) != S_OK)
		{
			pScrollPattern = NULL;
		}

		if(pScrollPattern == NULL)
		{
			//Release IUIAutomationElement
			pElem->Release();
			pElem = NULL;

			return false;
		}

		//Great .. we've found a scrollPattern in the subtree

		*ppScrollElem = new NScrollElem();

		pElem->get_CachedName(&(*ppScrollElem)->name);
		pElem->get_CachedBoundingRectangle(&(*ppScrollElem)->bounds);
		pElem->get_CachedControlType(&(*ppScrollElem)->controlType);

		(*ppScrollElem)->pScrollPattern = pScrollPattern;
		(*ppScrollElem)->parentIsScrollable = false; //dont't care

		BOOL verticalScrollable;
		BOOL horizontalScrollable;
		pScrollPattern->get_CachedVerticallyScrollable(&verticalScrollable);
		pScrollPattern->get_CachedHorizontallyScrollable(&horizontalScrollable);
		
		(*ppScrollElem)->isHorizontalScrollable = (horizontalScrollable == TRUE);
		(*ppScrollElem)->isVerticalScrollable = (verticalScrollable == TRUE);	

		//Release IUIAutomationElement
		pElem->Release();
		pElem = NULL;

		return true;
	}
	else
	{
		return false;
	}
}

void  NUIAutomation::AddFocusChangedEventHandler(NFocusedElementProvider* pFocusedElementProvider)
{
	if(pFocusedElementProvider != NULL && this->pComplexElemCache != NULL)
	{		
		pAutomation->AddFocusChangedEventHandler(this->pComplexElemCache, (IUIAutomationFocusChangedEventHandler*) pFocusedElementProvider);	
	}
}


void  NUIAutomation::RemoveFocusChangedEventHandler(NFocusedElementProvider* pFocusedElementProvider)
{
	if(pFocusedElementProvider != NULL)
	{	
		pAutomation->RemoveFocusChangedEventHandler(pFocusedElementProvider);	

		//pFocusChangedEventHandler = NULL;	
	}
}
