#pragma once

#include <list>

#include "NUIAutomation.h"
#include "NElem.h"
#include "NEventProvider.h"
#include "NFocusedElementProvider.h"

using namespace Native::UIAutomation;
using namespace std;

using namespace System;
using namespace System::Threading;
using namespace System::Drawing;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

// ----------------------------------------------------------------------------

namespace Phoebit
{
	namespace UIAutomation 
	{
		public ref struct UIElement
			{
				String^ name;
				String^ className;
				Int32	ControlType;
				System::Drawing::Rectangle^ Bounds;
			};

		public ref class UISpy
		{
		public:
			UISpy();
			~UISpy(); // release managed resource, calls finalizer
			!UISpy(); // release unmanaged resource

		public:
			
			//Search function. Triggers a point and region search
			bool StartSearch(Point position, System::Drawing::Rectangle screenBounds);
			//bool StopSearch();

			// Indicates wheter search is completed
			bool UISearchAtPointHasFinished();
			//bool UISearchInRegionHasFinished();
			
			bool WaitElement( [Out]UIElement^ %element);			

			// Results of position search
			Drawing::Rectangle GetBounds(); //Result available after position search
			bool IsEditControl();
			
			// Results of region search			
			//bool UiInRegionIsAmbiguous(double expectedGazeAccuracy, double ppm, IntPtr desktopHandle); //Result available after Region search

			// scrolling Results
			bool IsControlScrollable();								

			bool ScrollDown();
			bool ScrollUp();	

			bool IsFocusOnEditControl();
	
		private:
				
			// position for point and region search. Used by workerthread
			Point _position;
			System::Drawing::Rectangle _screenBounds;
			
			//volatile bool _breaksearch;
			volatile bool _kill;

			cli::array<Point>^ _spiralPatternList;
			cli::array<Point>^ _iconPatternList;

			//contains the element found by the SearchAtPoint-Thread
			NComplexElem* pElemAtPoint;
			//contains the elements found by the SearchInRegion-Thread				
			cli::array<NComplexElem*>^ _elemCollectionInRegion;

			//not used
			NScrollElem* pScollElem;

			NFocusedElementProvider* pFocusTracker;
	
		private:			
			//Whitelist of special controls in partner applications
			bool IsSpecialEditControl(NComplexElem* elem);

			//Blacklist
			enum class BlackListControlID { Office2010Close };

			//Blacklist of controls which returns wrong properties
			bool IsFuckedUpControl(NComplexElem* elem, BlackListControlID* id);
			bool HandleBlackListControl(NComplexElem* pClonedElem, BlackListControlID id);		
	
		private:
			//Worker thread
			void SearchThread();
			//void SearchAtRegionThread(Object^ params);

			// called by worker thread
			void SearchAtPointByNativeAutomation(NUIAutomation* pUIAutomation);
			//void SearchAtPointByNativeAutomation(NUIAutomation* pUIAutomation, NComplexElem* pElem, int idx);

			//void SearchInRegionByNativeAutomation(NUIAutomation* pUIAutomation); //no longer used
			//void SearchInRegionByNativeAutomationMultithreaded(NUIAutomation* pUIAutomation);								
			

			//Kill threads. Clean up. Called by Destructor
			void Kill();

		private:
			//worker thread 
			Thread^ _threadUISearch;

			// global event to start search
			AutoResetEvent^ _startEventTriggerSearch;

			// synchronization events for SearchAtPoint and SearchInRegion			
			ManualResetEvent^ _finishEventSearchAtPoint;
			//ManualResetEvent^ _finishEventSearchInRegion;		

			//array<Thread^>^ _threadRegionSearch;
			//array<AutoResetEvent^>^ _startEventTriggerRegionSearch;
			//array<ManualResetEvent^>^ _finishEventTriggerRegionSearch;			

			//Mutex^ _regionArrayMutex;			
		};
	}	
}
