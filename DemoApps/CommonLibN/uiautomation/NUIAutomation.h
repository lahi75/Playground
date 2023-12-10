// ----------------------------------------------------------------------------
//
//		Important: Do not use that class from an UI Thread!
//
// ----------------------------------------------------------------------------

#pragma once

#include <UIAutomation.h>
#include "NScrollElem.h"
#include "NComplexElem.h"
#include "NFocusedElementProvider.h"

// ----------------------------------------------------------------------------

namespace Native
{
	namespace UIAutomation
	{
		class NUIAutomation
		{
		public:
			NUIAutomation();
			~NUIAutomation(); // release unmanaged resources

		public:		
			bool SearchAtPositionUsingCache(POINT pt, NComplexElem* elem);
			bool SearchAtPositionUsingCache(POINT pt, NScrollElem* elem);	

			bool SearchSubtree(IUIAutomationElement* rootElement, NScrollElem** scrollElem);

			bool RegisterMenuClosedEvent(IUIAutomationEventHandler* callback);

			void AddFocusChangedEventHandler(NFocusedElementProvider* pFocusedElementProvider);
			void RemoveFocusChangedEventHandler(NFocusedElementProvider* pFocusedElementProvider);
					
		private:
		
			IUIAutomation* pAutomation;		
			IUIAutomationCacheRequest *pComplexElemCache;
			IUIAutomationCacheRequest *pScrollElemCache;		

			IUIAutomationCondition* pScrollPropertyCondition; //Required by NEventProvider
		};	
	}
}