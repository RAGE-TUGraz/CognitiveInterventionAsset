/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union�s Horizon
  2020 research and innovation programme under grant agreement No 644187.
  You may obtain a copy of the License at
  
      http://www.apache.org/licenses/LICENSE-2.0
  
  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
  
  This software has been created in the context of the EU-funded RAGE project.
  Realising and Applied Gaming Eco-System (RAGE), Grant agreement No 644187, 
  http://rageproject.eu/

  Development was done by Cognitive Science Section (CSS) 
  at Knowledge Technologies Institute (KTI)at Graz University of Technology (TUGraz).
  http://kti.tugraz.at/css/

  Created by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
*/
namespace CognitiveInterventionAssetNameSpace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AssetManagerPackage;
    using AssetPackage;
    using System.Xml.Serialization;
    using System.IO;
    using System.Xml;/// <summary>
                     /// An asset.
                     /// </summary>
    public class CognitiveInterventionAsset : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private CognitiveInterventionAssetSettings settings = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CognitiveInterventionAsset.Asset class.
        /// </summary>
        public CognitiveInterventionAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            // 
            settings = new CognitiveInterventionAssetSettings();

            //preventing multiple asset creation
            if (AssetManager.Instance.findAssetsByClass(this.Class).Count > 1)
            {
                this.Log(Severity.Error, "There is only one instance of the CognitiveInterventionAssetSettings permitted!");
                throw new Exception("EXCEPTION: There is only one instance of the CognitiveInterventionAssetSettings permitted!");
            }

        }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Gets or sets options for controlling the operation.
        /// </summary>
        ///
        /// <remarks>   Besides the toXml() and fromXml() methods, we never use this property but use
        ///                it's correctly typed backing field 'settings' instead. </remarks>
        /// <remarks> This property should go into each asset having Settings of its own. </remarks>
        /// <remarks>   The actual class used should be derived from BaseAsset (and not directly from
        ///             ISetting). </remarks>
        ///
        /// <value>
        /// The settings.
        /// </value>
        public override ISettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = (value as CognitiveInterventionAssetSettings);
            }
        }

        #endregion Properties
        #region Methods

        // Your code goes here.

        /// <summary>
        /// Performing all test of the cognitive intervention asset.
        /// </summary>
        public void performAllTests()
        {
            CognitiveInterventionHandler.Instance.performAllTests();
        }

        /// <summary>
        /// Methode for setting the Method for handling cognitive interventions
        /// </summary>
        /// <param name="del"> Method (signature: void del(string interventionInstance)) performed if an intervention is needed. </param>
        public void setInterventionDelegate(CognitiveInterventionDelegate del)
        {
            CognitiveInterventionHandler.Instance.cognitiveInterventionDelegate = del;
        }

        /// <summary>
        /// Method for sending direct traces to the asset
        /// </summary>
        /// <param name="trace"></param>
        public void sendTrace(string trace)
        {
            CognitiveInterventionHandler.Instance.addNewTrack(trace);
        }

        #endregion Methods

        #region InternalMethods
        /// <summary>
        /// Method for reading the Asset settings.
        /// </summary>
        /// <returns></returns>
        internal CognitiveInterventionAssetSettings getSettings()
        {
            return (this.settings);
        }
        #endregion InternalMethods

    }

}