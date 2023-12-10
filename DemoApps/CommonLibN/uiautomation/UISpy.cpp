#include "pch.h"
#include "UISpy.h"

using namespace System::Runtime::InteropServices; // for native <-> managed transitions

#include <map>
#include <vector>

using namespace Phoebit::UIAutomation;

// ----------------------------------------------------------------------------------------------
// Ctor
// ----------------------------------------------------------------------------------------------
// initalize uispy stuff
// ----------------------------------------------------------------------------------------------
UISpy::UISpy()
{
	int REGION_THREADS_COUNT = 36;

	_threadUISearch = gcnew Thread(gcnew ThreadStart(this, &UISpy::SearchThread));
	_threadUISearch->Name = "UI Automation Thread";
	_threadUISearch->SetApartmentState(System::Threading::ApartmentState::MTA); // background thread must be initialized as MTA
	_threadUISearch->IsBackground = true;


	//initialize region search threads
	/*
	_threadRegionSearch = gcnew array<Thread^>(REGION_THREADS_COUNT);

	for(int i = 0; i < _threadRegionSearch->Length; i++)
	{
		_threadRegionSearch[i] = gcnew Thread(gcnew ParameterizedThreadStart(this, &UISpy::SearchAtRegionThread));
		_threadRegionSearch[i]->Name = "UI Automation Region " + i.ToString() + " Thread";
		_threadRegionSearch[i]->SetApartmentState(System::Threading::ApartmentState::MTA);
		_threadRegionSearch[i]->IsBackground = true;
	}

	_regionArrayMutex = gcnew Mutex();
	*/

	
	_kill = false;
//	_breaksearch = false;

	/*
	_elemCollectionInRegion = gcnew array<NComplexElem*>(REGION_THREADS_COUNT);
	for(int i = 0; i < _elemCollectionInRegion->Length; i++)
	{
		_elemCollectionInRegion[i] = NULL;
	}
	*/

	pElemAtPoint = NULL;
	pFocusTracker = NULL;

	/*
	_spiralPatternList = gcnew array<Point>
    {
        //new Point(0,0), 
        Point(-25,0), Point(25,0), Point(0, -25), Point(0,25),
        Point(-50,0), Point(50,0), Point(0, -50), Point(0,50),
        Point(-25,-25), Point(25,-25), Point(25,25), Point(-25, 25),
        Point(-50,-50), Point(50,-50), Point(50,50), Point(-50, 50),
        Point(-75,-25), Point(-25,-75), Point(25,-75), Point(75, -25),
        Point(75,25), Point(25,75), Point(-25,75), Point(-75, 25)
    };	

	_iconPatternList = gcnew array<Point> //75 x 75 pixel -> 36 points
	{
		Point(-36,-36), Point(-21,-36), Point(-6, -36), Point(7,-36), Point(22,-36), Point(37,-36),
		Point(-36,-21), Point(-21,-21), Point(-6, -21), Point(7,-21), Point(22,-21), Point(37,-21),
		Point(-36,-6), Point(-21,-6), Point(-6, -6), Point(7,-6), Point(22,-6), Point(37,-6),
		Point(-36,7), Point(-21,7), Point(-6, 7), Point(7,7), Point(22,7), Point(37,7),
		Point(-36,22), Point(-21,22), Point(-6, 22), Point(7,22), Point(22,22), Point(37,22),
		Point(-36,37), Point(-21,37), Point(-6, 37), Point(7,37), Point(22,37), Point(37,37)
	};
	*/

	//Init synchronization events
	_startEventTriggerSearch = gcnew AutoResetEvent(false);
	_finishEventSearchAtPoint = gcnew ManualResetEvent(true);	
	//_finishEventSearchInRegion = gcnew ManualResetEvent(true);

	/*
	_startEventTriggerRegionSearch = gcnew array<AutoResetEvent^>(REGION_THREADS_COUNT);
	for(int i = 0; i < _startEventTriggerRegionSearch->Length; i++)
	{
		_startEventTriggerRegionSearch[i] = gcnew AutoResetEvent(false);
	}
	*/
	/*
	_finishEventTriggerRegionSearch = gcnew array<ManualResetEvent^>(REGION_THREADS_COUNT);
	for(int i = 0; i < _finishEventTriggerRegionSearch->Length; i++)
	{
		_finishEventTriggerRegionSearch[i] = gcnew ManualResetEvent(true);
	}*/

	
	//Start Region Threads
	/*
	for(int i = 0; i < _threadRegionSearch->Length; i++)
	{
		_threadRegionSearch[i]->Start(gcnew array<Object^>{_startEventTriggerRegionSearch[i], _finishEventTriggerRegionSearch[i], i});
	}
	*/

	//Start worker thread
	_threadUISearch->Start();	
}

// ----------------------------------------------------------------------------------------------
// ~UISpy
// ----------------------------------------------------------------------------------------------
// release managed resources
// ----------------------------------------------------------------------------------------------
UISpy::~UISpy()
{
	//Kill managed worker thread
	Kill();

	this->!UISpy();
}

// ----------------------------------------------------------------------------------------------
// !UISpy
// ----------------------------------------------------------------------------------------------
// release unmanaged resources
// ----------------------------------------------------------------------------------------------
UISpy::!UISpy()
{			
}

// ----------------------------------------------------------------------------------------------
// StartSearch
// ----------------------------------------------------------------------------------------------
// Signals Worker-Thread.
// Returns false if the worker thread is busy because of a previous search, otherwise true.
// ----------------------------------------------------------------------------------------------
bool UISpy::StartSearch(Point position, System::Drawing::Rectangle screenBounds)
{
	//previous search finished?
	if(_finishEventSearchAtPoint->WaitOne(0) /*&& _finishEventSearchInRegion->WaitOne(0)*/)
	{
		//update position for search
		_position = position;	
		_screenBounds = screenBounds;

		//reset synchronization events
		_finishEventSearchAtPoint->Reset();
		//_finishEventSearchInRegion->Reset();
		//_breaksearch = false;

		//Start search
		_startEventTriggerSearch->Set();		

		return true;
	}
	else
	{
		return false;
	}
}

// ----------------------------------------------------------------------------------------------
// StopSearch.
// We maybe have to implement a timeout here so we can kill and restart the UI-Automation-Thread if timeout has been expired!! (after 10 seconds)??? 
// ...but at the moment the function is NON-Blocking ... otherwise we get a deadlock here.
// ----------------------------------------------------------------------------------------------
// Stops the Search.
// Returns true if the search has been stopped. If the return value is false the worker thread is busy and couldn't stop the search immediately.
// If the return value is false the next call of StartSearch may be fail, because the thread is still trying to stop the current search.
// ----------------------------------------------------------------------------------------------
/*
bool UISpy::StopSearch()
{
	//tells UI-Automation thread to abort the search.
	//_breaksearch = true;

	return _finishEventSearchAtPoint->WaitOne(0)/* && _finishEventSearchInRegion->WaitOne(0);
}
*/
// ----------------------------------------------------------------------------------------------
// UISearchAtPointHasFinished
// ----------------------------------------------------------------------------------------------
// Returns true if SearchAtPosition-Thread is ready, otherwise false.
// ----------------------------------------------------------------------------------------------
bool UISpy::UISearchAtPointHasFinished()
{
	return _finishEventSearchAtPoint->WaitOne(0);
}

// ----------------------------------------------------------------------------------------------
// UISearchInRegionHasFinished
// ----------------------------------------------------------------------------------------------
// Returns true if SearchInRegion-Thread is ready, otherwise false.
// ----------------------------------------------------------------------------------------------
//bool UISpy::UISearchInRegionHasFinished()
//{
//	return _finishEventSearchInRegion->WaitOne(0);
//}


void UISpy::Kill()
{
	_kill = true;
	//_breaksearch = true;


	// Kill search thread
	_startEventTriggerSearch->Set();		

	if( _finishEventSearchAtPoint->WaitOne(200) == false )
		_threadUISearch->Interrupt();	

	
	//kill SearchInRegion Thread
	/*
	if(_finishEventSearchInRegion->WaitOne(0))
	{		
		_startEventTriggerSearch->Set();		
	}
	else
	{
		//Wait 100 ms		
		if(_finishEventSearchInRegion->WaitOne(100))
		{
			_startEventTriggerSearch->Set();
		}
		else
		{
			for(int i = 0; i < _threadRegionSearch->Length; i++)
			{
				_threadRegionSearch[i]->Interrupt();
			}

			_threadUISearch->Interrupt();	

			
		}
	}
	*/

	//Delete memory of region elements
	/*
	for(int i= 0; i< _elemCollectionInRegion->Length; i++)
	{
		if(_elemCollectionInRegion[i] != NULL)
		{
			delete _elemCollectionInRegion[i];	
			_elemCollectionInRegion[i] = NULL;
		}
	}*/
}


bool UISpy::IsEditControl()
{
	if (_finishEventSearchAtPoint->WaitOne(0))
	{
		if (pElemAtPoint != NULL)
		{
			//show keyboard only if we have found an Edit control and ValuePattern is not readonly, otherwise we got to many false positives
			bool hasWritableText = pElemAtPoint->controlType == UIA_EditControlTypeId && pElemAtPoint->hasWritableText;

			if(!hasWritableText)
			{
				//check special cases
				hasWritableText = IsSpecialEditControl(pElemAtPoint);				
			}

			return hasWritableText;
		}
	}

	return false;
}

bool UISpy::IsSpecialEditControl(NComplexElem* elem)
{	
	return 		
		(elem->controlType == UIA_PaneControlTypeId && wcscmp(elem->className,L"_WwG") == 0) ||           //Microsoft Word 2010 Document
		(elem->hasWritableText && wcscmp(elem->className, L"MozillaWindowClass") == 0) ||                 //Thunderbird mail text and Firefox Search textfield
		(elem->controlType == UIA_PaneControlTypeId && wcscmp(elem->className, L"TChatRichEdit") == 0) || //Skype Chat control

		false; //Add more controls here					
}

bool UISpy::IsFuckedUpControl(NComplexElem* elem, BlackListControlID* id)
{
	if(elem->controlType == UIA_ButtonControlTypeId && wcscmp(elem->className,L"NetUIAppFrameHelper") == 0) //Microsoft Office 2010 Close Button
	{
		*id = BlackListControlID::Office2010Close;
		return true;
	}

	//add more controls here

	return false;	
}


bool UISpy::HandleBlackListControl(NComplexElem* pClonedElem, BlackListControlID id)
{
	if(pClonedElem != NULL)
	{
		switch(id)
		{
		case BlackListControlID::Office2010Close:
			
			System::Drawing::Rectangle^ minimizeBounds = gcnew System::Drawing::Rectangle(pClonedElem->bounds.left, pClonedElem->bounds.top,(int)((pClonedElem->bounds.right - pClonedElem->bounds.left) / 3.0f) , pClonedElem->bounds.bottom - pClonedElem->bounds.top);
			System::Drawing::Rectangle^ maximizeBounds = gcnew System::Drawing::Rectangle(pClonedElem->bounds.left + (int)((pClonedElem->bounds.right - pClonedElem->bounds.left) / 3.0f), pClonedElem->bounds.top, (int)((pClonedElem->bounds.right - pClonedElem->bounds.left) / 3.0f), pClonedElem->bounds.bottom - pClonedElem->bounds.top);
			System::Drawing::Rectangle^ closeBounds = gcnew System::Drawing::Rectangle(pClonedElem->bounds.left + (int)( 2.0f * (pClonedElem->bounds.right - pClonedElem->bounds.left) / 3.0f), pClonedElem->bounds.top, (int)((pClonedElem->bounds.right - pClonedElem->bounds.left) / 3.0f), pClonedElem->bounds.bottom - pClonedElem->bounds.top);
			System::Drawing::Rectangle^ correctedBounds;

			if(minimizeBounds->Contains(_position))
			{
				correctedBounds = minimizeBounds;
			}
			else if (maximizeBounds->Contains(_position))
			{
				correctedBounds = maximizeBounds;
			}
			else if (closeBounds->Contains(_position))
			{
				correctedBounds = closeBounds;
			}
			else
				return false;

			pClonedElem->bounds.left = correctedBounds->X;
			pClonedElem->bounds.top = correctedBounds->Y;
			pClonedElem->bounds.right = correctedBounds->X + correctedBounds->Width;
			pClonedElem->bounds.bottom = correctedBounds->Y + correctedBounds->Height;

			return true;
		}

		return true;
	}
	
	return false;
}
						
bool UISpy::WaitElement([Out]UIElement^ %element)
{		
	if (_finishEventSearchAtPoint->WaitOne(1000))
	{
		if (pElemAtPoint != NULL)
		{		
			System::Drawing::Rectangle^ r = GetBounds();

			element->Bounds = r;
			element->className =  Marshal::PtrToStringUni( (IntPtr)pElemAtPoint->className);
			element->name = Marshal::PtrToStringUni( (IntPtr)pElemAtPoint->name );
			element->ControlType = pElemAtPoint->controlType;					
				
		}

		if(pFocusTracker != NULL)
		{
			NElem* pFocusedElement = new NElem();

			if(pFocusTracker->GetLastFocusedElement(pFocusedElement))
			{
				if(pFocusedElement != NULL)
				{
					 //result = (pFocusedElement->controlType == UIA_EditControlTypeId);	

					String^ name = gcnew String(pFocusedElement->name);					
				}
			}

			delete pFocusedElement;
		}

	}
	else
		return false;

	return true;
}


System::Drawing::Rectangle UISpy::GetBounds()
{
	System::Drawing::Rectangle bounds;

	if (_finishEventSearchAtPoint->WaitOne(0))
	{
		if (pElemAtPoint != NULL)
		{		
			BlackListControlID id;

			if(IsFuckedUpControl(pElemAtPoint, &id))
			{	
				NComplexElem* clonedElem = new NComplexElem();
				
				pElemAtPoint->Clone(*clonedElem);
								
				HandleBlackListControl(clonedElem, id);

				bounds.X = clonedElem->bounds.left;
				bounds.Y = clonedElem->bounds.top;
				bounds.Width = clonedElem->bounds.right - bounds.X;
				bounds.Height = clonedElem->bounds.bottom - bounds.Y;

				//Clean up					
				delete clonedElem;				
			}
			else 
			{
				bounds.X = pElemAtPoint->bounds.left;
				bounds.Y = pElemAtPoint->bounds.top;
				bounds.Width = pElemAtPoint->bounds.right - bounds.X;
				bounds.Height = pElemAtPoint->bounds.bottom - bounds.Y;
			}
		}
	}

	return bounds;
}

bool UISpy::IsControlScrollable()
{
	if (_finishEventSearchAtPoint->WaitOne(0))
	{
		if (pScollElem != NULL)
		{
			return (pScollElem->isHorizontalScrollable || pScollElem->isVerticalScrollable);
		}
	}

	return false;
}

bool UISpy::ScrollDown()
{
	if (_finishEventSearchAtPoint->WaitOne(0))
	{
		if (pScollElem != NULL)
		{
			if(pScollElem->isVerticalScrollable)
			{
				if(pScollElem->pScrollPattern != NULL)
				{
					double currentValue;

					pScollElem->pScrollPattern->get_CurrentVerticalScrollPercent(&currentValue);

					currentValue+=10;

					if(currentValue > 100)
						currentValue = 100;

					return (pScollElem->pScrollPattern->Scroll(ScrollAmount_NoAmount, ScrollAmount_SmallIncrement) == S_OK);
					//return (pScollElem->pScrollPattern->SetScrollPercent(-1, currentValue) == S_OK);
				}
			}
		}
	}

	return false;
}

bool UISpy::ScrollUp()
{
	if (_finishEventSearchAtPoint->WaitOne(0))
	{
		if (pScollElem != NULL)
		{
			if(pScollElem->isVerticalScrollable)
			{
				if(pScollElem->pScrollPattern != NULL)
				{
					double currentValue;

					pScollElem->pScrollPattern->get_CurrentVerticalScrollPercent(&currentValue);

					currentValue-=10;

					if(currentValue < 0)
						currentValue = 0;

					return (pScollElem->pScrollPattern->Scroll(ScrollAmount_NoAmount, ScrollAmount_SmallDecrement) == S_OK);
					//return (pScollElem->pScrollPattern->SetScrollPercent(-1, currentValue) == S_OK);
				}
			}
		}
	}

	return false;
}

/*
bool UISpy::UiInRegionIsAmbiguous(double expectedGazeAccuracy, double ppm, IntPtr desktopHandle)
{
	if (_finishEventSearchInRegion->WaitOne(0))
	{
		if(_elemCollectionInRegion->Length == 0)
			return false;

		if(pElemAtPoint == NULL)
			return true;

		bool isAmbiguous = false;
				
		// 
		if(!(
			//if one of these condition are true, the magnifier isn't shown if no other control in region was found.
			pElemAtPoint->controlType == UIA_ButtonControlTypeId ||
			pElemAtPoint->controlType == UIA_CheckBoxControlTypeId ||
			pElemAtPoint->controlType == UIA_ComboBoxControlTypeId ||
			pElemAtPoint->controlType == UIA_EditControlTypeId ||
			pElemAtPoint->controlType == UIA_ListItemControlTypeId ||
			pElemAtPoint->controlType == UIA_MenuItemControlTypeId ||				
			pElemAtPoint->controlType == UIA_TreeItemControlTypeId ||
			pElemAtPoint->controlType == UIA_PaneControlTypeId ||
			pElemAtPoint->controlType == UIA_HyperlinkControlTypeId ||
			(pElemAtPoint->controlType == UIA_ListControlTypeId && wcscmp(pElemAtPoint->name, L"Desktop") == 0 ) ||
			(pElemAtPoint->controlType == UIA_ListControlTypeId && pElemAtPoint->windowHandle == (HWND)desktopHandle.ToPointer()) ||
			(pElemAtPoint->controlType == UIA_ImageControlTypeId && pElemAtPoint->bounds.right - pElemAtPoint->bounds.left > 30 && pElemAtPoint->bounds.bottom - pElemAtPoint->bounds.top > 30)
			)
			)
		{
			isAmbiguous = true;
		} // the ui element at mouse position is one of the above UI controls
		else 
		{

			double elemAtPointCenterX = (pElemAtPoint->bounds.right + pElemAtPoint->bounds.left) / 2;
			double elemAtPointCenterY = (pElemAtPoint->bounds.bottom + pElemAtPoint->bounds.top) / 2;
			vector<int> avgSizeVec;
			vector<double> distancesVec;			
			
			// go through the collection of elements present in the search region 
			for each (NElem* elem in _elemCollectionInRegion)
			{		
				if(elem == NULL)
					continue;

				// consider only elements that belong to the UI controls below
				if(
					elem->controlType == UIA_ButtonControlTypeId ||
					elem->controlType == UIA_CheckBoxControlTypeId ||
					elem->controlType == UIA_ComboBoxControlTypeId ||
					elem->controlType == UIA_EditControlTypeId ||
					elem->controlType == UIA_ListItemControlTypeId ||
					elem->controlType == UIA_MenuItemControlTypeId ||				
					elem->controlType == UIA_TreeItemControlTypeId
					)
				{
					if(*elem != *pElemAtPoint)
					{
						// caculate element's center
						RECT elemBounds = elem->bounds;
						double elemCenterX = (elemBounds.right + elemBounds.left) / 2;
						double elemCenterY = (elemBounds.bottom + elemBounds.top) / 2;
						// skip element if element's center is very close to elementAtPoint's center (i.e most likely one is a subelement of the other)
						if( (Math::Abs(elemCenterX - elemAtPointCenterX ) < 10) && 
							(Math::Abs(elemCenterY - elemAtPointCenterY ) < 10 ) )
							continue;
						// calculte distance from element's center to gaze point
						double distance = Math::Sqrt( (_position.X - elemCenterX) * (_position.X - elemCenterX) + (_position.Y - elemCenterY) * (_position.Y - elemCenterY) );
						distancesVec.push_back(distance);

					}
				}			
			}

			// no other controls beside the one at mouse/gaze position, make isAmbiguous false
			if(distancesVec.size() == 0)
			{
				isAmbiguous = false;
			}
			else
			{
				
				// calculate the reference distance: the distance from the elemAtPoint center to gaze point
				double refDistance = Math::Sqrt( (_position.X - elemAtPointCenterX) * (_position.X - elemAtPointCenterX) + (_position.Y - elemAtPointCenterY) * (_position.Y - elemAtPointCenterY) );

				// get the minimum distance from all accepted elements
				int minIndex = -1;
				double minDistance = 10000;
				double minAccuracy = 15;
				for(int i = 0; i < (int)distancesVec.size(); i++)
				{
					if(distancesVec[i] <= minDistance)
					{
						minDistance = distancesVec[i];
						minIndex = i;
					}
				}

				
				double expectedGazeAccuracyMm = 10 * expectedGazeAccuracy; // 
				double expectedGazeAccuracyPx = expectedGazeAccuracyMm * ppm; // mm
			
				// if minimum distance is below average gaze accuracy make isAmbiguous true (i.e. another element is too close to gaze point to make an unambiguos decisions)
				if(minDistance >= expectedGazeAccuracyPx) 
				{
					// model a normal distribution based on gaze accuracy
					// based on this distribuition we will weigh the distances to the gaze point 
					double gazeAccuracyDistributionMean =  expectedGazeAccuracyPx / 2; // assumption: normal distribution 
					double gazeAccuracyDistributionStDev = expectedGazeAccuracyPx;				
					// assign weights to minimum and referrence distance based on the gaze accuracy distribution
					double normalizationFactor = Math::Sqrt(2 * Math::PI * gazeAccuracyDistributionStDev * gazeAccuracyDistributionStDev);
					double refDistanceWeight = Math::Exp( - (refDistance - gazeAccuracyDistributionMean) * (refDistance - gazeAccuracyDistributionMean) / ( 2 * gazeAccuracyDistributionStDev * gazeAccuracyDistributionStDev)) / normalizationFactor;
					double minDistanceWeight = Math::Exp( - (minDistance - gazeAccuracyDistributionMean) * (minDistance - gazeAccuracyDistributionMean) / ( 2 * gazeAccuracyDistributionStDev * gazeAccuracyDistributionStDev)) / normalizationFactor;
					double weightThreshold = 1 / (5  * normalizationFactor); // a fifth of the peak level of the gaussian function
				
					// if the weight of the minimum distance is below the weight of reference distance by less that the threshold or is simply above
					// the weight of the reference distance make isambiguous true
					if (refDistanceWeight - minDistanceWeight < weightThreshold)
					{
						isAmbiguous = true;
					}
				}
				else
				{
					isAmbiguous = true;
				}

			}

		}
		
		
		return isAmbiguous; 		
	}
	else
	{
		//Should never happen. Use UISearchInRegionHasFinished before calling this function.
		return true;
	}
}
*/
bool UISpy::IsFocusOnEditControl()
{	
	bool result = true;

	if(pFocusTracker != NULL)
	{
		NElem* pFocusedElement = new NElem();

		if(pFocusTracker->GetLastFocusedElement(pFocusedElement))
		{
			if(pFocusedElement != NULL)
			{
				 result = (pFocusedElement->controlType == UIA_EditControlTypeId);	

				 String^ name = gcnew String(pFocusedElement->name);				 
			}
		}

		delete pFocusedElement;
	}

	return result;	
}

// ----------------------------------------------------------------------------------------------
// SearchThread
// ----------------------------------------------------------------------------------------------
// Worker Thread
// ----------------------------------------------------------------------------------------------
void UISpy::SearchThread()
{
	//Initialize thread in MTA apartment
	HRESULT comApartment = CoInitializeEx(NULL, COINIT_MULTITHREADED);

	//Native UI Automation
	NUIAutomation* pUIAutomation = new NUIAutomation();

	pElemAtPoint = NULL;	
	
	
	//focus tracker
	pFocusTracker = new NFocusedElementProvider();	
	pUIAutomation->AddFocusChangedEventHandler(pFocusTracker);
	

	try
	{

		while(_startEventTriggerSearch->WaitOne() && !_kill)
		{
			//--------- position search -----------------

			SearchAtPointByNativeAutomation(pUIAutomation);

			_finishEventSearchAtPoint->Set();

			//--------- Region search -----------------

			//SearchInRegionByNativeAutomation(pUIAutomation);
			//SearchInRegionByNativeAutomationMultithreaded(pUIAutomation);

			//_finishEventSearchInRegion->Set();
		}	


		if(_kill)
		{
			//trigger all region threads
			//for(int i=0; i < _startEventTriggerRegionSearch->Length; i++)
			//{
				//_startEventTriggerRegionSearch[i]->Set();
			//}
		}
	}
	catch(ThreadAbortException^)
	{
		//thread has been killed
	}
	catch(ThreadInterruptedException^)
	{
		//thread has been killed
	}

	if(pUIAutomation != NULL)
		pUIAutomation->RemoveFocusChangedEventHandler(pFocusTracker);

	if(pFocusTracker != NULL)
	{
		pFocusTracker->Release();
		delete pFocusTracker;
		pFocusTracker = NULL;
	}

	//clean up
	if(pUIAutomation != NULL)
	{
		delete pUIAutomation;
		pUIAutomation = NULL;
	}

	if(pElemAtPoint != NULL)
	{
		delete pElemAtPoint;
		pElemAtPoint = NULL;
	}	

	if(comApartment == S_OK || comApartment == S_FALSE)
		CoUninitialize();
}


// ----------------------------------------------------------------------------------------------
// Region SearchThread. This Thread is created 36 Times
// ----------------------------------------------------------------------------------------------
// Worker Thread
// ----------------------------------------------------------------------------------------------
/*
void UISpy::SearchAtRegionThread(Object^ params)
{
	array<Object^>^ arr = (array<Object^>^)params;

	AutoResetEvent^ startEventTriggerRegionSearch = (AutoResetEvent^)arr[0];
	ManualResetEvent^ finishEventSearchInRegion = (ManualResetEvent^)arr[1];
	int idx = (int)arr[2];


	//Initialize thread in MTA apartment
	HRESULT comApartment = CoInitializeEx(NULL, COINIT_MULTITHREADED);

	//Native UI Automation
	NUIAutomation* pUIAutomation = new NUIAutomation();

	NComplexElem* pElem = NULL;	

	try
	{

		while(startEventTriggerRegionSearch->WaitOne() && !_kill)
		{
			//--------- position search -----------------

			SearchAtPointByNativeAutomation(pUIAutomation, pElem, idx);

			finishEventSearchInRegion->Set();
		}	

	}
	catch(ThreadAbortException^)
	{
		//thread has been killed
	}
	catch(ThreadInterruptedException^)
	{
		//thread has been killed
	}

	//clean up
	if(pUIAutomation != NULL)
	{
		delete pUIAutomation;
		pUIAutomation = NULL;
	}

	if(pElem != NULL)
	{
		delete pElem;
		pElem = NULL;
	}	

	if(comApartment == S_OK || comApartment == S_FALSE)
		CoUninitialize();

}
*/

 
 

// ----------------------------------------------------------------------------------------------
// SearchAtPointByNativeAutomation
// ----------------------------------------------------------------------------------------------
// Worker Thread
// ----------------------------------------------------------------------------------------------
void UISpy::SearchAtPointByNativeAutomation(NUIAutomation* pUIAutomation)
{
	if(pElemAtPoint != NULL)
		delete pElemAtPoint;

	pElemAtPoint = new NComplexElem();

	POINT p;
	p.x = _position.X;
	p.y = _position.Y;

	

	if(!pUIAutomation->SearchAtPositionUsingCache(p, pElemAtPoint))
	{
		delete pElemAtPoint;
		pElemAtPoint = NULL;
	}

	//String^ name = gcnew String(pElemAtPoint->name);
	
	///		
	
	//System::Diagnostics::Debug::WriteLine( name );

	//???Find scroll element here???

/*	if(pScollElem != NULL)
	{
		delete pScollElem;
		pScollElem = NULL;
	}
		
	if(pElemAtPoint != NULL && pElemAtPoint->parentIsScrollable)
	{
		//Search parents
		pScollElem = new NScrollElem();
			
		if(!pUIAutomation->SearchAtPositionUsingCache(p, pScollElem))
		{			
			delete pScollElem;
			pScollElem = NULL;				
		}			
	} */
}

// ----------------------------------------------------------------------------------------------
// SearchAtPointByNativeAutomation
// ----------------------------------------------------------------------------------------------
// Search at a specified point and add the result to the RegionArray
// ----------------------------------------------------------------------------------------------
/*
void UISpy::SearchAtPointByNativeAutomation(NUIAutomation* pUIAutomation, NComplexElem* pElem, int regionArrayIdx)
{
	if(pElem != NULL)
		delete pElem;
	
	pElem = new NComplexElem();	

	POINT p;			
	p.x = _position.X + _iconPatternList[regionArrayIdx].X;				
	p.y = _position.Y + _iconPatternList[regionArrayIdx].Y;

	if(!_screenBounds.Contains(p.x, p.y) || !pUIAutomation->SearchAtPositionUsingCache(p, pElem))
	{
		delete pElem;
		pElem = NULL;
	}

	_regionArrayMutex->WaitOne();

	_elemCollectionInRegion[regionArrayIdx] = pElem;

	_regionArrayMutex->ReleaseMutex();
}
*/

// ----------------------------------------------------------------------------------------------
// SearchInRegionByNativeAutomation.
// ----------------------------------------------------------------------------------------------
// Worker Thread
// 
/*
void UISpy::SearchInRegionByNativeAutomationMultithreaded(NUIAutomation* pUIAutomation)
{
	//Clear all items in last list before searching
	for(int i= 0; i< _elemCollectionInRegion->Length; i++)
	{
		//do not delete memory here!!! Elements will be deleted by the worker threads!
		_elemCollectionInRegion[i] = NULL;		
	}

	//reset synchronization trigger
	for(int i = 0; i < _finishEventTriggerRegionSearch->Length; i++)
	{
		_finishEventTriggerRegionSearch[i]->Reset();
	}

	//Trigger region search in all threads
	for(int i = 0; i < _startEventTriggerRegionSearch->Length; i++)
	{
		_startEventTriggerRegionSearch[i]->Set();
	}

	//Wait for all threads
	WaitHandle::WaitAll(_finishEventTriggerRegionSearch);

	//Region search complete. Delete duplicated items in list
			
	//remove duplicated items
	for(int i = 0; i <_elemCollectionInRegion->Length; i++)
	{
		bool isUnique = true;

		for(int j = i - 1; j >= 0; j--)
		{
			if(_elemCollectionInRegion[i] == NULL)
			{
				isUnique = false;
				break;
			}

			if(_elemCollectionInRegion[j] != NULL && *_elemCollectionInRegion[j] == *_elemCollectionInRegion[i])
			{
				isUnique = false;
				break;
			}
		}

		//remove duplicated item
		if(_elemCollectionInRegion[i] != NULL && !isUnique)
		{			
			//do not delete memory here!!! Elements will be deleted by the worker threads!
			_elemCollectionInRegion[i] = NULL;			
		}
	}
}
*/

// ----------------------------------------------------------------------------------------------
// SearchInRegionByNativeAutomation. This function is no longer used
// ----------------------------------------------------------------------------------------------
// Worker Thread
// ----------------------------------------------------------------------------------------------
/*
void UISpy::SearchInRegionByNativeAutomation(NUIAutomation* pUIAutomation)
{
	vector<NComplexElem*> elemList;
	int count = 0;

	for (int i = 0; i < _iconPatternList->Length; i++)
    {
        if (_breaksearch)
        {
            break;
        }

		POINT p;			
		p.x = _position.X + _iconPatternList[i].X;				
		p.y = _position.Y + _iconPatternList[i].Y;

		NComplexElem* pElem = new NComplexElem();
                
		//search at point only if point is inside of primary screen!
		if(!_screenBounds.Contains(p.x, p.y) || ! pUIAutomation->SearchAtPositionUsingCache(p, pElem))
		{
			delete pElem;
			pElem = NULL;
		}

		if (pElem != NULL)
		{

			bool unique = true;

			//check whether element is already in list
			for ( vector<NComplexElem*>::iterator iterator = elemList.begin(); iterator != elemList.end(); iterator++ )
			{
				if(**iterator == *pElem)
				{
					unique = false;
					break;				
				}
			}

			if(unique)
			{
				elemList.push_back(pElem);
				count ++;
			}
			else
			{
				delete pElem;
				pElem = NULL;
			}
		}                                              
	}		
		
	//Delete memory of all items in last list
	for(int i= 0; i< _elemCollectionInRegion->Length; i++)
	{
		delete _elemCollectionInRegion[i];
	}


	//copy new elements	
	_elemCollectionInRegion = gcnew array<NComplexElem*>(count);
		 
	int idx=0;

	for (vector<NComplexElem*>::iterator iterator = elemList.begin(); iterator != elemList.end(); iterator++, idx++ )
	{
		_elemCollectionInRegion[idx] = *iterator;			
	}
}
*/