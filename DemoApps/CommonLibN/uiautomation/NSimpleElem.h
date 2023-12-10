// ----------------------------------------------------------------------------

#pragma once

#include <Windows.h>
#include "NElem.h"

// ----------------------------------------------------------------------------

namespace Native
{
	namespace UIAutomation
	{
		struct NSimpleElem : NElem
		{
			NSimpleElem()
			{
				parentIsScrollable = false;
			}

			virtual ~NSimpleElem()
			{
				Clear();		
			}

			//Clear allocated resources of element
			void Clear()
			{
				//call base function
				NElem::Clear();
			}

			//Clone element
			void Clone(NSimpleElem& targetElement)
			{
				//call base function
				NElem::Clone(targetElement);
			
				targetElement.parentIsScrollable = this->parentIsScrollable;
			}

			bool operator==(const NSimpleElem& e)
			{		
				if(&e == NULL)
					return false;

				return (NElem::operator==(e) && parentIsScrollable == e.parentIsScrollable);			
			}

			bool operator!=(const NSimpleElem& e)
			{			
				return !operator==(e);
			}

			bool parentIsScrollable;
		};
	}
}

// ----------------------------------------------------------------------------