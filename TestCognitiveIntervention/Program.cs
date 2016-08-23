/*
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
*/

using AssetManagerPackage;
using AssetPackage;
using CognitiveInterventionAssetNameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnitTestCognitiveIntervention;

namespace TestCognitiveIntervention
{
    class Program
    {
        static void Main(string[] args)
        {
            AssetManager.Instance.Bridge = new Bridge();

            CognitiveInterventionAsset cia = new CognitiveInterventionAsset();

            //setting feedback method
            CognitiveInterventionDelegate cognitiveInterventionDelegate = (interventionType, interventionInstance) => Console.WriteLine("DelegateLogging: " + interventionType + ", "+ interventionInstance);
            cia.setInterventionDelegate(cognitiveInterventionDelegate);

            //start Test
            TestCognitiveInterventionAsset tcia = new TestCognitiveInterventionAsset();
            //tcia.performAllTests();

            CognitiveInterventionAssetSettings cias = new CognitiveInterventionAssetSettings();
            cias.XMLFileLocation = "demoGameCognitiveIntervention.xml";
            cia.Settings = cias;

            cia.sendTrace("failure additionGS");
            cia.sendTrace("failure sortGS");
            cia.sendTrace("failure additionGS");

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }

    class TestCognitiveInterventionAsset
    {
        #region Fields

        public int timeToDeactivationOfNodesInMiliSec = 1000;

        #endregion Fields
        #region HelperMethods

        /// <summary>
        /// Logging functionality for Test
        /// </summary>
        /// <param name="msg"> Message to be logged </param>
        public void log(String msg, Severity severity = Severity.Information)
        {
            ILog logger = (ILog)AssetManager.Instance.Bridge;
            logger.Log(severity, "[CIA Test]" + msg);
        }

        /// <summary>
        /// Method returning the Asset
        /// </summary>
        /// <returns> The Asset</returns>
        public CognitiveInterventionAsset getCIA()
        {
            return (CognitiveInterventionAsset)AssetManager.Instance.findAssetByClass("CognitiveInterventionAsset");
        }

        /// <summary>
        /// Method for setting the data source
        /// </summary>
        /// <param name="data"> The structure to be set </param>
        public void setDataSource(XMLCognitiveInterventionData data)
        {
            string fileId = "CognitiveInterventionAssetTestId.xml";

            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                log("Storing Cognitive intervention data to File.");
                ids.Save(fileId, data.toXmlString());
            }
            else
                log("No IDataStorage - Bridge implemented!", Severity.Warning);

            //change Settings to load local file
            CognitiveInterventionAssetSettings newCIAS = new CognitiveInterventionAssetSettings();
            newCIAS.XMLFileLocation = fileId;
            ((CognitiveInterventionAsset)AssetManager.Instance.findAssetByClass("CognitiveInterventionAsset")).Settings = newCIAS;
        }

        /// <summary>
        /// Creates an example XMLCognitiveInterventionData structure.
        /// </summary>
        /// <returns> An example XMLCognitiveInterventionData structure. </returns>
        internal XMLCognitiveInterventionData createExampleXMLCognitiveInterventionData()
        {
            //create example XML structure
            XMLCognitiveInterventionData xmlStructure = new XMLCognitiveInterventionData();

            //create interventions and instances
            XMLCognitiveInterventionTypeList typeList = new XMLCognitiveInterventionTypeList();
            XMLCognitiveInterventionType type1 = new XMLCognitiveInterventionType("type1");
            XMLCognitiveInterventionType type2 = new XMLCognitiveInterventionType("type2");

            XMLCognitiveInterventionInstanceList instanceList1 = new XMLCognitiveInterventionInstanceList();
            XMLCognitiveInterventionInstanceList instanceList2 = new XMLCognitiveInterventionInstanceList();

            //fill lists...
            XMLCognitiveInterventionInstance instance11 = new XMLCognitiveInterventionInstance("instance 1 of type 1");
            XMLCognitiveInterventionInstance instance21 = new XMLCognitiveInterventionInstance("instance 2 of type 1");
            XMLCognitiveInterventionInstance instance12 = new XMLCognitiveInterventionInstance("instance 1 of type 2");
            XMLCognitiveInterventionInstance instance22 = new XMLCognitiveInterventionInstance("instance 2 of type 2");

            instanceList1.cognitiveInterventionInstanceList = new List<XMLCognitiveInterventionInstance>();
            instanceList1.cognitiveInterventionInstanceList.Add(instance11);
            instanceList1.cognitiveInterventionInstanceList.Add(instance21);

            instanceList2.cognitiveInterventionInstanceList = new List<XMLCognitiveInterventionInstance>();
            instanceList2.cognitiveInterventionInstanceList.Add(instance12);
            instanceList2.cognitiveInterventionInstanceList.Add(instance22);

            type1.cognitiveInterventionInstanceList = instanceList1;
            type2.cognitiveInterventionInstanceList = instanceList2;

            typeList.cognitiveInterventionType = new List<XMLCognitiveInterventionType>();


            typeList.cognitiveInterventionType.Add(type1);
            typeList.cognitiveInterventionType.Add(type2);


            //create tree structure
            XMLCognitiveInterventionNodeList nodeList = new XMLCognitiveInterventionNodeList();
            nodeList.cognitiveInterventionNodes = new List<XMLCognitiveInterventionNode>();

            //create nodes
            XMLCognitiveInterventionNode node0 = new XMLCognitiveInterventionNode("node0");
            XMLCognitiveInterventionEdgeList edgeList0 = new XMLCognitiveInterventionEdgeList();
            edgeList0.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n0 = new XMLCognitiveInterventionEdge("goTo1", "exclusive", "node1");
            edgeList0.cognitiveInterventionEdges.Add(edge1n0);
            XMLCognitiveInterventionEdge edge2n0 = new XMLCognitiveInterventionEdge("goTo4", "exclusive", "node4");
            edgeList0.cognitiveInterventionEdges.Add(edge2n0);
            node0.cognitiveInterventionEdgeList = edgeList0;
            node0.activationDuration = this.timeToDeactivationOfNodesInMiliSec;

            XMLCognitiveInterventionNode node1 = new XMLCognitiveInterventionNode("node1");
            XMLCognitiveInterventionEdgeList edgeList1 = new XMLCognitiveInterventionEdgeList();
            edgeList1.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n1 = new XMLCognitiveInterventionEdge("goTo2", "exclusive", "node2");
            edgeList1.cognitiveInterventionEdges.Add(edge1n1);
            XMLCognitiveInterventionEdge edge2n1 = new XMLCognitiveInterventionEdge("goTo3", "exclusive", "node3");
            edgeList1.cognitiveInterventionEdges.Add(edge2n1);
            node1.cognitiveInterventionEdgeList = edgeList1;
            node1.activationDuration = this.timeToDeactivationOfNodesInMiliSec;

            XMLCognitiveInterventionNode node2 = new XMLCognitiveInterventionNode("node2");
            XMLCognitiveInterventionEdgeList edgeList2 = new XMLCognitiveInterventionEdgeList();
            edgeList2.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n2 = new XMLCognitiveInterventionEdge("goTo6", "exclusive", "node6");
            edgeList2.cognitiveInterventionEdges.Add(edge1n2);
            node2.cognitiveInterventionEdgeList = edgeList2;
            node2.activationDuration = this.timeToDeactivationOfNodesInMiliSec;

            XMLCognitiveInterventionNode node6 = new XMLCognitiveInterventionNode("node6");
            XMLCognitiveInterventionEdgeList edgeList6 = new XMLCognitiveInterventionEdgeList();
            edgeList6.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n6 = new XMLCognitiveInterventionEdge("goTo7", "exclusive", "node7");
            edgeList6.cognitiveInterventionEdges.Add(edge1n6);
            node6.cognitiveInterventionEdgeList = edgeList6;
            node6.activationDuration = this.timeToDeactivationOfNodesInMiliSec;

            XMLCognitiveInterventionNode node3 = new XMLCognitiveInterventionNode("node3");
            XMLCognitiveInterventionEdgeList edgeList3 = new XMLCognitiveInterventionEdgeList();
            edgeList3.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n3 = new XMLCognitiveInterventionEdge("goTo5", "exclusive", "node5");
            edgeList3.cognitiveInterventionEdges.Add(edge1n3);
            node3.cognitiveInterventionEdgeList = edgeList3;
            node3.activationDuration = this.timeToDeactivationOfNodesInMiliSec;

            XMLCognitiveInterventionNode node4 = new XMLCognitiveInterventionNode("node4");
            XMLCognitiveInterventionEdgeList edgeList4 = new XMLCognitiveInterventionEdgeList();
            edgeList4.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n4 = new XMLCognitiveInterventionEdge("goTo5", "exclusive", "node5");
            edgeList4.cognitiveInterventionEdges.Add(edge1n4);
            node4.cognitiveInterventionEdgeList = edgeList4;
            node4.activationDuration = this.timeToDeactivationOfNodesInMiliSec;
            node4.interventionType = "type2";

            XMLCognitiveInterventionNode node7 = new XMLCognitiveInterventionNode("node7");
            XMLCognitiveInterventionEdgeList edgeList7 = new XMLCognitiveInterventionEdgeList();
            edgeList7.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            node7.activationDuration = this.timeToDeactivationOfNodesInMiliSec;
            node7.interventionType = "type1";

            XMLCognitiveInterventionNode node5 = new XMLCognitiveInterventionNode("node5");
            XMLCognitiveInterventionEdgeList edgeList5 = new XMLCognitiveInterventionEdgeList();
            edgeList5.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            node5.activationDuration = this.timeToDeactivationOfNodesInMiliSec;



            //add nodes
            nodeList.cognitiveInterventionNodes.Add(node0);
            nodeList.cognitiveInterventionNodes.Add(node1);
            nodeList.cognitiveInterventionNodes.Add(node2);
            nodeList.cognitiveInterventionNodes.Add(node3);
            nodeList.cognitiveInterventionNodes.Add(node4);
            nodeList.cognitiveInterventionNodes.Add(node5);
            nodeList.cognitiveInterventionNodes.Add(node6);
            nodeList.cognitiveInterventionNodes.Add(node7);

            xmlStructure.cognitiveInterventionNodeList = nodeList;
            xmlStructure.cognitiveInterventionTypeList = typeList;
            xmlStructure.cognitiveInterventionStartingNode = "node0";

            return xmlStructure;
        }
        
        #endregion HelperMethods
        #region TestMethods

        /// <summary>
        /// Performing all test of the cognitive intervention asset.
        /// </summary>
        internal void performAllTests()
        {
            log("Start performing Cognitive Intervention Asset tests:");
            CognitiveInterventionDelegate cognitiveInterventionDelegate = (interventionType, interventionInstance) => log("DelegateLogging: " + interventionType + ", " + interventionInstance);
            performTest1();
            performTest2();
            performTest3();
            performTest4();
            performTest5();
            performTest6();
            log("Cognitive Intervention Asset tests ended.");
        }

        /// <summary>
        /// Creates and outputs example XMLCognitiveInterventionData
        /// </summary>
        internal void performTest1()
        {
            log("Start Test 1");
            log(createExampleXMLCognitiveInterventionData().toXmlString());
            log("End Test 1");
        }

        /// <summary>
        /// Creating example CI-structure and only sending "useful"  traces.
        /// </summary>
        internal void performTest2()
        {
            log("Start Test 2");
            setDataSource(createExampleXMLCognitiveInterventionData());

            getCIA().sendTrace("goTo1");
            getCIA().sendTrace("goTo2");
            getCIA().sendTrace("goTo6");
            getCIA().sendTrace("goTo7");
            
            log("End Test 2");
        }

        /// <summary>
        /// Creating example CI-structure and not only sending "useful"  traces.
        /// </summary>
        internal void performTest3()
        {
            log("Start Test 3");
            setDataSource(createExampleXMLCognitiveInterventionData());

            getCIA().sendTrace("goTo1");
            getCIA().sendTrace("goTo2");
            getCIA().sendTrace("goTo2");
            getCIA().sendTrace("goTo5");
            getCIA().sendTrace("goTo6");
            getCIA().sendTrace("goTo7");
            
            log("End Test 3");
        }

        /// <summary>
        /// Creating example CI-structure and not only sending "useful"  traces plus starting new trace series.
        /// </summary>
        internal void performTest4()
        {
            log("Start Test 4");
            setDataSource(createExampleXMLCognitiveInterventionData());

            getCIA().sendTrace("goTo1");
            getCIA().sendTrace("goTo2");
            getCIA().sendTrace("goTo4");
            getCIA().sendTrace("goTo5");
            getCIA().sendTrace("goTo6");
            getCIA().sendTrace("goTo1");
            getCIA().sendTrace("goTo3");
            getCIA().sendTrace("goTo7");

            log("End Test 4");
        }

        /// <summary>
        /// Creating example CI-structure and waiting till one node is deactivated.
        /// </summary>
        internal void performTest5()
        {
            log("Start Test 5");
            setDataSource(createExampleXMLCognitiveInterventionData());

            getCIA().sendTrace("goTo1");
            Thread.Sleep(2 * this.timeToDeactivationOfNodesInMiliSec);
            getCIA().sendTrace("goTo7");

            log("End Test 5");
        }

        /// <summary>
        /// Method Testing if derivation from anticipated behaviour intervention triggers
        /// </summary>
        internal void performTest6()
        {
            log("Start Test 6");
            setDataSource(createExampleXMLCognitiveInterventionData());

            getCIA().sendTrace("goTo4");
            Thread.Sleep(2 * this.timeToDeactivationOfNodesInMiliSec);
            getCIA().sendTrace("goTo7");

            log("Start Test 6");
        }

        #endregion TestMethods

    }

}
