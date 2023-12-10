// ----------------------------------------------------------------------------
//
// Description: class holds reference of scroll pattern
//
// ----------------------------------------------------------------------------

#pragma once

#include <Windows.h>
#include <UIAutomationClient.h>
#include "NSimpleElem.h"

// ----------------------------------------------------------------------------

namespace Native
{
	namespace UIAutomation
	{
		struct NScrollElem : NSimpleElem
		{
			NScrollElem()
			{
				pScrollPattern = NULL;
				isHorizontalScrollable = false;
				isVerticalScrollable = false;
			}

			virtual ~NScrollElem()
			{
				if(pScrollPattern != NULL)
				{
					pScrollPattern->Release();
					pScrollPattern = NULL;
				
				}			
			}	

			//Clone element
			void Clone(NScrollElem& targetElement)
			{
				//call base function
				NSimpleElem::Clone(targetElement);
			
				targetElement.isHorizontalScrollable = this->isHorizontalScrollable;
				targetElement.isVerticalScrollable = this->isVerticalScrollable;
				targetElement.pScrollPattern = this->pScrollPattern;
			}
		
			bool operator==(const NScrollElem& e)
			{	
				if(&e == NULL)
					return false;

				return (NSimpleElem::operator==(e) && isHorizontalScrollable == e.isHorizontalScrollable && isVerticalScrollable == e.isVerticalScrollable);
			}

			bool operator!=(const NScrollElem& e)
			{			
				return !operator==(e);			
			}

			IUIAutomationScrollPattern* pScrollPattern;

			bool isHorizontalScrollable;
			bool isVerticalScrollable;
		};
	}
}

// ----------------------------------------------------------------------------