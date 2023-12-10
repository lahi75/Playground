
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// ----------------------------------------------------------------------------

namespace Phoebit.Desktop
{ 
    /// <summary>
    /// collection of all AOIs
    /// </summary>
    [Serializable]
    public class AoiList
    {
        Hashtable _aoiList = new Hashtable();        

        /// <summary>
        /// add a new AOI if it's unique
        /// </summary>        
        public bool Add(Aoi aoi)
        {          
            if (!_aoiList.ContainsKey(aoi.GetHashCode()))
            {
                _aoiList.Add(aoi.GetHashCode(), aoi);
                return true;
            }

            return false;
        }

        /// <summary>
        /// clears the list
        /// </summary>
        public void Clear()
        {
            _aoiList.Clear();            
        }            

        /// <summary>
        /// access the AOI collection
        /// </summary>      
        public Hashtable Aois
        {
            get
            {
                return _aoiList;
            }
                  
        }        
    }
}

// ----------------------------------------------------------------------------