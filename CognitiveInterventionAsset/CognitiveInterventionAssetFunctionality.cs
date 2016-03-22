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
  Changed by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed on: 2016-02-22
*/


using AssetManagerPackage;
using AssetPackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CognitiveInterventionAssetNameSpace
{
    /// <summary>
    /// Singelton Class for handling Cognitive Interventions
    /// </summary>
    internal class CognitiveInterventionHandler
    {

        #region Fields

        /// <summary>
        /// Instance of the CognitiveInterventionHandler - Singelton pattern
        /// </summary>
        private static CognitiveInterventionHandler instance;

        /// <summary>
        /// If true, logging is done.
        /// </summary>
        internal bool doLogging = true;

        /// <summary>
        /// Instance of the CognitiveInterventionAsset.
        /// </summary>
        internal CognitiveInterventionAsset cognitiveInterventionAsset;

        /// <summary>
        /// Instance of the CognitiveInterventionTree - base for working on track pattern recognition.
        /// </summary>
        internal CognitiveInterventionTree cognitiveInterventionTree = null;

        /// <summary>
        /// Delegate handling the intervention feedback
        /// </summary>
        internal CognitiveInterventionDelegate cognitiveInterventionDelegate = null;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Private ctor - Singelton pattern
        /// </summary>
        private CognitiveInterventionHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the CognitiveInterventionHandler - Singelton pattern
        /// </summary>
        public static CognitiveInterventionHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CognitiveInterventionHandler();
                }
                return instance;
            }
        }

        #endregion Properties
        #region Methods

        /// <summary>
        /// Method returning an instance of the CognitiveInterventionAsset.
        /// </summary>
        /// <returns> Instance of the CognitiveInterventionAsset </returns>
        internal CognitiveInterventionAsset getCIA()
        {
            if (cognitiveInterventionAsset == null)
                cognitiveInterventionAsset = (CognitiveInterventionAsset)AssetManager.Instance.findAssetByClass("CognitiveInterventionAsset");
            return (cognitiveInterventionAsset);
        }

        /// <summary>
        /// Return instance of CognitiveInterventionTree for the algorithmus
        /// </summary>
        /// <returns> instance of CognitiveInterventionTree </returns>
        internal CognitiveInterventionTree getCognitiveInterventionTree()
        {
            if (cognitiveInterventionTree == null)
                loadCognitiveInterventionTree();
            return cognitiveInterventionTree;
        }
        
        /// <summary>
        /// Loading the CognitiveInterventionTree for the Asset Handler
        /// </summary>
        internal void loadCognitiveInterventionTree()
        {
            loggingCI("Loading CognitiveIntervention XML-datastructure.");
            CognitiveInterventionAssetSettings cias = getCIA().getSettings();

            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                if (!ids.Exists(cias.XMLFileLocation))
                {
                    loggingCI("File " + cias.XMLFileLocation + " not found for loading CognitiveIntervention XML-datastructure.", Severity.Error);
                    throw new Exception("EXCEPTION: File " + cias.XMLFileLocation + " not found for loading CognitiveIntervention XML-datastructure.");
                }

                loggingCI("Loading CognitiveIntervention XML-datastructure from File.");
                cognitiveInterventionTree = new CognitiveInterventionTree(getDatastructureFromXmlString(ids.Load(cias.XMLFileLocation)));
            }
            else
            {
                loggingCI("IDataStorage bridge absent for loading CognitiveIntervention XML-datastructure.", Severity.Error);
                throw new Exception("EXCEPTION: IDataStorage bridge absent for loading CognitiveIntervention XML-datastructure.");
            }
            
        }

        /// <summary>
        /// uses new received track to update the cognitive intervention tree
        /// </summary>
        /// <param name="track"> track received from the game. </param>
        internal void addNewTrack(string track)
        {
            loggingCI("New track added: '"+track+"'.");
            CognitiveInterventionTree tree = getCognitiveInterventionTree();

            List<string> listOfActiveNodeIds = tree.getListOfActiveNodeIds();
            foreach(string nodeId in listOfActiveNodeIds)
            {
                
                if (tree.getCognitiveInterventionNodeById(nodeId).edges.ContainsKey(track))
                    if (tree.getCognitiveInterventionNodeById(nodeId).edges[track].active)
                        tree.setActive(tree.getCognitiveInterventionNodeById(nodeId), track);
            }
            tree.logActiveNodes();
        }

        /// <summary>
        /// Method for deserilizing the cognitive intervention XML datastructure.
        /// </summary>
        /// <param name="str"> the xml-string to be desirilized. </param>
        /// <returns> The desirilized data structure. </returns>
        internal XMLCognitiveInterventionData getDatastructureFromXmlString(String str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XMLCognitiveInterventionData));
            using (TextReader reader = new StringReader(str))
            {
                XMLCognitiveInterventionData result = (XMLCognitiveInterventionData)serializer.Deserialize(reader);
                return (result);
            }
        }

        #endregion Methods
        #region Testmethods

        /// <summary>
        /// Method for logging (Diagnostics).
        /// </summary>
        /// 
        /// <param name="msg"> Message to be logged. </param>
        internal void loggingCI(String msg, Severity severity = Severity.Information)
        {
            if (doLogging)
                getCIA().Log(severity, "[CIA]: " + msg);
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

            XMLCognitiveInterventionNode node1 = new XMLCognitiveInterventionNode("node1");
            XMLCognitiveInterventionEdgeList edgeList1 = new XMLCognitiveInterventionEdgeList();
            edgeList1.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n1 = new XMLCognitiveInterventionEdge("goTo2", "exclusive","node2");
            edgeList1.cognitiveInterventionEdges.Add(edge1n1);
            XMLCognitiveInterventionEdge edge2n1 = new XMLCognitiveInterventionEdge("goTo3", "exclusive", "node3");
            edgeList1.cognitiveInterventionEdges.Add(edge2n1);
            node1.cognitiveInterventionEdgeList = edgeList1;

            XMLCognitiveInterventionNode node2 = new XMLCognitiveInterventionNode("node2");
            XMLCognitiveInterventionEdgeList edgeList2 = new XMLCognitiveInterventionEdgeList();
            edgeList2.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n2 = new XMLCognitiveInterventionEdge("goTo6", "exclusive", "node6");
            edgeList2.cognitiveInterventionEdges.Add(edge1n2);
            node2.cognitiveInterventionEdgeList = edgeList2;

            XMLCognitiveInterventionNode node6 = new XMLCognitiveInterventionNode("node6");
            XMLCognitiveInterventionEdgeList edgeList6 = new XMLCognitiveInterventionEdgeList();
            edgeList6.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n6 = new XMLCognitiveInterventionEdge("goTo7", "exclusive", "node7");
            edgeList6.cognitiveInterventionEdges.Add(edge1n6);
            node6.cognitiveInterventionEdgeList = edgeList6;

            XMLCognitiveInterventionNode node3 = new XMLCognitiveInterventionNode("node3");
            XMLCognitiveInterventionEdgeList edgeList3 = new XMLCognitiveInterventionEdgeList();
            edgeList3.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n3 = new XMLCognitiveInterventionEdge("goTo5", "exclusive", "node5");
            edgeList3.cognitiveInterventionEdges.Add(edge1n3);
            node3.cognitiveInterventionEdgeList = edgeList3;

            XMLCognitiveInterventionNode node4 = new XMLCognitiveInterventionNode("node4");
            XMLCognitiveInterventionEdgeList edgeList4 = new XMLCognitiveInterventionEdgeList();
            edgeList4.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            XMLCognitiveInterventionEdge edge1n4 = new XMLCognitiveInterventionEdge("goTo5", "exclusive", "node5");
            edgeList4.cognitiveInterventionEdges.Add(edge1n4);
            node4.cognitiveInterventionEdgeList = edgeList4;

            XMLCognitiveInterventionNode node7 = new XMLCognitiveInterventionNode("node7");
            XMLCognitiveInterventionEdgeList edgeList7 = new XMLCognitiveInterventionEdgeList();
            edgeList7.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            node7.interventionType = "type1";

            XMLCognitiveInterventionNode node5 = new XMLCognitiveInterventionNode("node5");
            XMLCognitiveInterventionEdgeList edgeList5 = new XMLCognitiveInterventionEdgeList();
            edgeList5.cognitiveInterventionEdges = new List<XMLCognitiveInterventionEdge>();
            node5.interventionType = "type2";



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

            return xmlStructure ;
        }

        /// <summary>
        /// Performing all test of the cognitive intervention asset.
        /// </summary>
        internal void performAllTests()
        {
            loggingCI("Start performing Cognitive Intervention Asset tests:");
            CognitiveInterventionDelegate oldDel = cognitiveInterventionDelegate;
            cognitiveInterventionDelegate = interventionInstance => loggingCI("DelegateLogging: "+ interventionInstance);
            performTest1();
            performTest2();
            performTest3();
            performTest4();
            cognitiveInterventionDelegate = oldDel;
            loggingCI("Cognitive Intervention Asset tests ended.");
        }

        /// <summary>
        /// Creates and outputs example XMLCognitiveInterventionData
        /// </summary>
        internal void performTest1()
        {
            loggingCI(createExampleXMLCognitiveInterventionData().toXmlString());
        }

        /// <summary>
        /// Creating example CI-structure and only sending "useful"  traces.
        /// </summary>
        internal void performTest2()
        {
            CognitiveInterventionTree oldTree = getCognitiveInterventionTree();
            CognitiveInterventionTree newTree = new CognitiveInterventionTree(createExampleXMLCognitiveInterventionData());
            this.cognitiveInterventionTree = newTree;

            this.addNewTrack("goTo1");
            this.addNewTrack("goTo2");
            this.addNewTrack("goTo6");
            this.addNewTrack("goTo7");

            this.cognitiveInterventionTree = oldTree;
        }

        /// <summary>
        /// Creating example CI-structure and not only sending "useful"  traces.
        /// </summary>
        internal void performTest3()
        {
            CognitiveInterventionTree oldTree = getCognitiveInterventionTree();
            CognitiveInterventionTree newTree = new CognitiveInterventionTree(createExampleXMLCognitiveInterventionData());
            this.cognitiveInterventionTree = newTree;

            this.addNewTrack("goTo1");
            this.addNewTrack("goTo2");
            this.addNewTrack("goTo2");
            this.addNewTrack("goTo5");
            this.addNewTrack("goTo6");
            this.addNewTrack("goTo7");

            this.cognitiveInterventionTree = oldTree;
        }

        /// <summary>
        /// Creating example CI-structure and not only sending "useful"  traces plus starting new trace series.
        /// </summary>
        internal void performTest4()
        {
            CognitiveInterventionTree oldTree = getCognitiveInterventionTree();
            CognitiveInterventionTree newTree = new CognitiveInterventionTree(createExampleXMLCognitiveInterventionData());
            this.cognitiveInterventionTree = newTree;

            this.addNewTrack("goTo1");
            this.addNewTrack("goTo2");
            this.addNewTrack("goTo4");
            this.addNewTrack("goTo5");
            this.addNewTrack("goTo6");
            this.addNewTrack("goTo1");
            this.addNewTrack("goTo3");
            this.addNewTrack("goTo7");

            this.cognitiveInterventionTree = oldTree;
        }

        #endregion Testmethods
    }

    /// <summary>
    /// Delegate - called with the intervention instance as argument
    /// </summary>
    /// <param name="interventionInstance"> intervention instance proposed by the asset</param>
    public delegate void CognitiveInterventionDelegate(string interventionInstance);

    /// <summary>
    /// Class managing the intervention activation.
    /// </summary>
    internal class CognitiveInterventionTree
    {
        #region Fields

        /// <summary>
        /// List containing all cogitive intervention nodes, each of which represent a state of activity flow from the game leading to an intervention.
        /// </summary>
        internal List<CognitiveInterventionNode> nodes = new List<CognitiveInterventionNode>();

        /// <summary>
        /// List containing 'active' nodes - meaning those state of activity which are currently taken on.
        /// </summary>
        internal List<CognitiveInterventionNode> activeNodes = new List<CognitiveInterventionNode>();

        /// <summary>
        /// Collection of all interventions and instances for the cognitive intervention asset.
        /// </summary>
        internal Dictionary<string, List<string>> interventions = new Dictionary<string, List<string>>();

        /// <summary>
        /// Dictionary for counting how often concrete intervention instances are triggered.
        /// </summary>
        internal Dictionary<string, int> interventionInstanceCounter = null;

        /// <summary>
        /// Node which is active all the time.
        /// </summary>
        internal CognitiveInterventionNode startingNode;

        #endregion Fields
        #region Constructors

        internal CognitiveInterventionTree(XMLCognitiveInterventionData data)
        {
            //fill interventions
            List<string> instances;
            foreach (XMLCognitiveInterventionType interventionType in data.cognitiveInterventionTypeList.cognitiveInterventionType)
            {
                instances = new List<string>();
                foreach (XMLCognitiveInterventionInstance instance in interventionType.cognitiveInterventionInstanceList.cognitiveInterventionInstanceList)
                    instances.Add(instance.instance);
                this.interventions[interventionType.typeId] = instances;
            }

            //create and add all nodes
                foreach (XMLCognitiveInterventionNode xmlNode in data.cognitiveInterventionNodeList.cognitiveInterventionNodes)
            {
                CognitiveInterventionNode newNode = new CognitiveInterventionNode(xmlNode.nodeId);
                newNode.interventionType = xmlNode.interventionType;
                this.nodes.Add(newNode);
            }
            //create and add all edges
            CognitiveInterventionNode node;
            CognitiveInterventionEdge newEdge;
            foreach (XMLCognitiveInterventionNode xmlNode in data.cognitiveInterventionNodeList.cognitiveInterventionNodes)
            {
                node = getCognitiveInterventionNodeById(xmlNode.nodeId);
                if (xmlNode.cognitiveInterventionEdgeList != null && xmlNode.cognitiveInterventionEdgeList.cognitiveInterventionEdges != null)
                {
                    foreach (XMLCognitiveInterventionEdge xmlEdge in xmlNode.cognitiveInterventionEdgeList.cognitiveInterventionEdges)
                    {
                        newEdge = new CognitiveInterventionEdge();
                        newEdge.successor = getCognitiveInterventionNodeById(xmlEdge.successorId);
                        newEdge.exclusive = xmlEdge.type.Equals("exclusive") ? true : false;
                        node.edges[xmlEdge.trackingId] = newEdge;
                    }
                }
                else
                {
                    node.edges = new Dictionary<string, CognitiveInterventionEdge>();
                }
            }

            this.startingNode = getCognitiveInterventionNodeById(data.cognitiveInterventionStartingNode);
            this.activeNodes.Add(this.startingNode);

        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// Find a Cognitive intervention node by id
        /// </summary>
        /// <param name="id"> Id of the cognitive intervention node to return. </param>
        /// <returns> cognitive intervention node. </returns>
        internal CognitiveInterventionNode getCognitiveInterventionNodeById(string id)
        {
            foreach (CognitiveInterventionNode node in this.nodes)
            {
                if (id.Equals(node.id))
                    return (node);
            }
            throw new Exception("An error occurred");
        }

        /// <summary>
        /// Deactivates a given node.
        /// </summary>
        /// <param name="nodeTodeactivate"> Node which gets deactivated </param>
        private void deactivateNode(CognitiveInterventionNode nodeToDeactivate)
        {
            CognitiveInterventionHandler.Instance.loggingCI("Deactivating Node '"+nodeToDeactivate.id+"'.");
            this.activeNodes.Remove(nodeToDeactivate);
        }

        /// <summary>
        /// Activates a given node.
        /// </summary>
        /// <param name="nodeToActivate"> Node which gets activated </param>
        private void activateNode(CognitiveInterventionNode nodeToActivate)
        {
            CognitiveInterventionHandler.Instance.loggingCI("Activating Node '" + nodeToActivate.id + "'.");

            if (!this.activeNodes.Contains(nodeToActivate))
                this.activeNodes.Add(nodeToActivate);

            foreach (string trackIterator in nodeToActivate.edges.Keys)
                nodeToActivate.edges[trackIterator].active = true;
            

            if (nodeToActivate.interventionType != null)
            {
                performIntervention(nodeToActivate.interventionType);
                deactivateNode(nodeToActivate);
            }
        }

        /// <summary>
        /// Method for setting a node active.
        /// </summary>
        /// <param name="startingNode"> Node from which the new active node is reached. </param>
        /// <param name="track"> Tracking id for setting the node active. </param>
        internal void setActive(CognitiveInterventionNode startingNode, string track)
        {
            CognitiveInterventionHandler.Instance.loggingCI("New update: starting from Node '"+startingNode.id+"' because of track '"+track+"'.");

            CognitiveInterventionEdge edge = startingNode.edges[track];

            //set edge inactive, if not started from starting node
            if(!this.startingNode.id.Equals(startingNode.id))
                edge.active = false;

            //deactivate node if it is an exclusive edge
            if (edge.exclusive && !this.startingNode.id.Equals(startingNode.id))
                deactivateNode(startingNode);

            //deactivate node if all edges are deactivated
            bool anActiveEdgeExists = false;
            foreach(string trackIterator in startingNode.edges.Keys)
                if (startingNode.edges[trackIterator].active)
                    anActiveEdgeExists = true;
            if (!anActiveEdgeExists && !this.startingNode.id.Equals(startingNode.id))
                deactivateNode(startingNode);

            //activate new edge
            activateNode(edge.successor);
        }

        /// <summary>
        /// Method for diagnostic logging.
        /// </summary>
        internal void logActiveNodes()
        {
            string txt = "";
            txt += "Active nodes:\n ";
            foreach (CognitiveInterventionNode node in this.activeNodes)
                txt += node.id+" | ";
            CognitiveInterventionHandler.Instance.loggingCI(txt);
        }

        /// <summary>
        /// Method for returning a List of active node ids.
        /// </summary>
        /// <returns> List of active node ids </returns>
        internal List<string> getListOfActiveNodeIds()
        {
            List<string> listToReturn = new List<string>();

            foreach (CognitiveInterventionNode node in activeNodes)
                listToReturn.Add(node.id);

            return listToReturn;
        }

        /// <summary>
        /// Method triggering an intervention if neccessary.
        /// </summary>
        /// <param name="interventionType"> String identifyier for the intervention type. </param>
        internal void performIntervention(string interventionType)
        {
            List<string> interventionInstances = this.interventions[interventionType];

            //initialize instance counter, if not done yet
            if (interventionInstanceCounter == null)
            {
                interventionInstanceCounter = new Dictionary<string, int>();
                foreach(string interventionTypeString in this.interventions.Keys)
                    foreach(string interventionInstanceString in this.interventions[interventionTypeString])
                        interventionInstanceCounter[interventionInstanceString] = 0;
            }

            //get those intervention, which are choosen the least often
            int leastOftenChosenInstanceNr = -1;
            foreach (string instance in interventionInstances)
                if (leastOftenChosenInstanceNr == -1 || leastOftenChosenInstanceNr > interventionInstanceCounter[instance])
                    leastOftenChosenInstanceNr = interventionInstanceCounter[instance];

            List<string> leastOftenChosenInstances = new List<string>();
            foreach (string instance in interventionInstances)
                if (interventionInstanceCounter[instance] == leastOftenChosenInstanceNr)
                    leastOftenChosenInstances.Add(instance);

            //choose random instance
            Random rnd = new Random();
            int pos = rnd.Next(leastOftenChosenInstances.Count);
            string chosenInstance = leastOftenChosenInstances[pos];

            //upate counter
            interventionInstanceCounter[chosenInstance]++;
            
            CognitiveInterventionHandler.Instance.loggingCI("Proposed intervention-instance: " + chosenInstance);
            if(CognitiveInterventionHandler.Instance.cognitiveInterventionDelegate == null)
            {
                CognitiveInterventionHandler.Instance.loggingCI("There was no method defined (via the asset method 'setInterventionDelegate') for handling cognitive intervention events!", Severity.Error);
                throw new Exception("EXCEPTION: There was no method defined (via the asset method 'setInterventionDelegate') for handling cognitive intervention events!");
            }
            CognitiveInterventionHandler.Instance.cognitiveInterventionDelegate(chosenInstance);
        }

        //TODO
        internal string toXMLString()
        {
            XMLCognitiveInterventionData xmlData = new XMLCognitiveInterventionData();
            //TODO....
            return null;
        }

        #endregion Methods
    }

    /// <summary>
    /// Class representing a node in the cognitive intervention tree
    /// </summary>
    internal class CognitiveInterventionNode
    {
        #region Fields

        /// <summary>
        ///  Unique id of the node.
        /// </summary>
        internal string id;

        /// <summary>
        /// Collection of edges to possible successors - stored as trackingId - Edge Dictionary.
        /// </summary>
        internal Dictionary<string, CognitiveInterventionEdge> edges = new Dictionary<string, CognitiveInterventionEdge>();

        /// <summary>
        /// Type of intervention that should be triggered when reaching this node.
        /// </summary>
        internal string interventionType;

        #endregion Fields
        #region Constructors

        internal CognitiveInterventionNode(string id)
        {
            this.id = id;
        }

        #endregion Constructors
    }

    /// <summary>
    /// Class representing a edge in the cognitive intervention tree.
    /// </summary>
    internal class CognitiveInterventionEdge
    {
        #region Fields

        /// <summary>
        /// Tracking id for which this edge is activated.
        /// </summary>
        //internal string trackingId;

        /// <summary>
        /// If true, all other edges of this node can not be activated after this edge gets activated.
        /// </summary>
        internal bool exclusive;

        /// <summary>
        /// Successor node for this edge.
        /// </summary>
        internal CognitiveInterventionNode successor;

        /// <summary>
        /// Set to false, if an node is set to active via this edge. Set to true, if the underlying node is set active. 
        /// </summary>
        internal bool active = true;

        #endregion Fields
    }


    #region Serialization

    /// <summary>
    /// Class containing all data for the cognitive intervention asset.
    /// </summary>
    [XmlRoot("cognitiveinterventiondata")]
    public class XMLCognitiveInterventionData
    {
        #region Fields

        /// <summary>
        /// Node which is active in the beginning.
        /// </summary>
        [XmlElement("cognitiveinterventionstartingnode")]
        public string cognitiveInterventionStartingNode { get; set; }

        /// <summary>
        /// Class containing all cognitive intervention nodes.
        /// </summary>
        [XmlElement("cognitiveinterventionnodes")]
        public XMLCognitiveInterventionNodeList cognitiveInterventionNodeList { get; set; }

        /// <summary>
        /// Class containing all cognitive intervention types.
        /// </summary>
        [XmlElement("cognitiveinterventiontypes")]
        public XMLCognitiveInterventionTypeList cognitiveInterventionTypeList { get; set; }


        #endregion Fields
        #region Methods

        /// <summary>
        /// Method for converting CognitiveInterventionData to a xml string.
        /// </summary>
        /// 
        ///<returns>
        /// A string representing the CognitiveInterventionData.
        /// </returns>
        public String toXmlString()
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(XMLCognitiveInterventionData));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, this);
                    String xml = stringWriter.ToString();

                    return (xml);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        #endregion Methods
    }

    public class XMLCognitiveInterventionTypeList
    {
        #region Fields

        /// <summary>
        /// List of all cognitive intervention types.
        /// </summary>
        [XmlElement("cognitiveinterventiontype")]
        public List<XMLCognitiveInterventionType> cognitiveInterventionType { get; set; }

        #endregion Fields
    }

    public class XMLCognitiveInterventionType
    {
        #region Fields

        /// <summary>
        /// Id of the cognitive intervention type.
        /// </summary>
        [XmlElement("typeid")]
        public string typeId { get; set; }

        /// <summary>
        /// List of all cognitive intervention instances to the corresponding type.
        /// </summary>
        [XmlElement("cognitiveinterventiontype")]
        public XMLCognitiveInterventionInstanceList cognitiveInterventionInstanceList { get; set; }

        #endregion Fields
        #region Constructors

        public XMLCognitiveInterventionType(){}

        public XMLCognitiveInterventionType(string id)
        {
            this.typeId = id;
        }

        #endregion Constructors
    }

    public class XMLCognitiveInterventionInstanceList
    {
        #region Fields

        /// <summary>
        /// List of all cognitive intervention instances.
        /// </summary>
        [XmlElement("cognitiveinterventioninstance")]
        public List<XMLCognitiveInterventionInstance> cognitiveInterventionInstanceList { get; set; }

        #endregion Fields
    }

    public class XMLCognitiveInterventionInstance
    {
        #region Fields

        /// <summary>
        /// A concrete cognitive intervention instance.
        /// </summary>
        [XmlElement("instance")]
        public string instance { get; set; }

        #endregion Fields
        #region Constructors

        public XMLCognitiveInterventionInstance() { }

        public XMLCognitiveInterventionInstance(string instance)
        {
            this.instance = instance;
        }

        #endregion Constructors
    }

    /// <summary>
    /// Class containing cognitive intervention nodes
    /// </summary>
    public class XMLCognitiveInterventionNodeList
    {
        #region Fields

        /// <summary>
        /// List of all cognitive intervention nodes.
        /// </summary>
        [XmlElement("cognitiveinterventionnode")]
        public List<XMLCognitiveInterventionNode> cognitiveInterventionNodes { get; set; }

        #endregion Fields
    }

    /// <summary>
    /// class containing one cognitive intervention node
    /// </summary>
    public class XMLCognitiveInterventionNode
    {
        #region Fields

        /// <summary>
        /// Id of the node
        /// </summary>
        [XmlElement("nodeid")]
        public string nodeId { get; set; }

        /// <summary>
        /// Class containing all cognitive intervention edges starting from this node.
        /// </summary>
        [XmlElement("cognitiveinterventionedgelist")]
        public XMLCognitiveInterventionEdgeList cognitiveInterventionEdgeList { get; set; }

        /// <summary>
        /// InterventionType to call when reaching this node
        /// </summary>
        [XmlElement("interventiontype")]
        public string interventionType { get; set; }

        #endregion Fields
        #region Constructore

        internal XMLCognitiveInterventionNode() { }

        internal XMLCognitiveInterventionNode(string id)
        {
            this.nodeId = id;
        }

        #endregion Constructors
    }

    /// <summary>
    /// class containing all cognitive intervention edges. 
    /// </summary>
    public class XMLCognitiveInterventionEdgeList
    {
        #region Fields

        /// <summary>
        /// List of all cognitive intervention edges.
        /// </summary>
        [XmlElement("cognitiveinterventionedge")]
        public List<XMLCognitiveInterventionEdge> cognitiveInterventionEdges { get; set; }

        #endregion Fields
    }

    /// <summary>
    /// Class representing a congnitive intervention edge.
    /// </summary>
    public class XMLCognitiveInterventionEdge
    {
        #region Fields

        /// <summary>
        /// Tracking id for which this edge is activated.
        /// </summary>
        [XmlElement("trackingid")]
        public string trackingId { get; set; }

        /// <summary>
        /// Can be 'exclusive' or 'parallel'. If 'exclusive' no other successor of this edge can be activated after activating this successor.
        /// </summary>
        [XmlElement("type")]
        public string type { get; set; }

        [XmlElement("successorid")]
        public string successorId { get; set; }

        #endregion Fields
        #region Constructor 

        internal XMLCognitiveInterventionEdge() { }

        internal XMLCognitiveInterventionEdge(string trackingId, string type, string successorId)
        {
            this.trackingId = trackingId;
            this.type = type;
            this.successorId = successorId;
        }

        #endregion Constructor
    }



    #endregion Serialization

}
