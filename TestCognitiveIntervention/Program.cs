﻿/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union’s Horizon
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
  Changed on: 2016-02-22
*/

using AssetManagerPackage;
using AssetPackage;
using CognitiveInterventionAssetNameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestCognitiveIntervention
{
    class Program
    {
        static void Main(string[] args)
        {
            AssetManager.Instance.Bridge = new Bridge();

            CognitiveInterventionAsset cia = new CognitiveInterventionAsset();

            //setting feedback method
            CognitiveInterventionDelegate cognitiveInterventionDelegate = interventionInstance => Console.WriteLine("DelegateLogging: " + interventionInstance);
            cia.setInterventionDelegate(cognitiveInterventionDelegate);

            cia.performAllTests();
            cia.sendTrace("goTo1");
            cia.sendTrace("goTo2");
            cia.sendTrace("goTo6");
            cia.sendTrace("goTo7");

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }


    class Bridge : IBridge, ILog, IDataStorage
    {
        #region IDataStorage
        public bool Delete(string fileId)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string fileId)
        {
            string filePath = @"C:\Users\mmaurer\Desktop\rageCsFiles\" + fileId;
            return (File.Exists(filePath));
        }

        public string[] Files()
        {
            throw new NotImplementedException();
        }

        public string Load(string fileId)
        {
            string path = @"C:\Users\mmaurer\Desktop\rageCsFiles\" + fileId;

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(path))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    return (line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Bridge: Error when reading file!");
            }

            return (null);
        }

        public void Save(string fileId, string fileData)
        {
            string filePath = @"C:\Users\mmaurer\Desktop\rageCsFiles\" + fileId;
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.Write(fileData);
            }
        }

        #endregion IDataStorage
        #region ILog

        public void Log(Severity severity, string msg)
        {
            Console.WriteLine("Bridge: " + msg);
        }

        #endregion ILog
    }
}
