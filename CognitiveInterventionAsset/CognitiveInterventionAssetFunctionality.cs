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
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CognitiveInterventionAssetNameSpace
{
    /// <summary>
    /// Singelton Class for handling Cognitive Interventions
    /// </summary>
    internal class CognitiveInterventionHandler
    {
        #region AlgorithmParameter
        
        #endregion AlgorithmParameter
        #region Fields

        /// <summary>
        /// If true, logging is done.
        /// </summary>
        internal bool doLogging = true;

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
        public CognitiveInterventionHandler() { }

        #endregion Constructors
        #region Properties
        #endregion Properties
        #region Methods

        /// <summary>
        /// Method returning an instance of the CognitiveInterventionAsset.
        /// </summary>
        /// <returns> Instance of the CognitiveInterventionAsset </returns>
        internal CognitiveInterventionAsset getCIA()
        {
            return CognitiveInterventionAsset.Instance;
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
            CognitiveInterventionAssetSettings cias = (CognitiveInterventionAssetSettings) getCIA().Settings;

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

            foreach (CognitiveInterventionNode node in this.cognitiveInterventionTree.nodes)
                node.wasActivatedThisUpdate = false;

            List<string> listOfActiveNodeIds = tree.getListOfActiveNodeIds();
            foreach(string nodeId in listOfActiveNodeIds)
            {
                CognitiveInterventionNode node = tree.getCognitiveInterventionNodeById(nodeId);

                if(node.isStillActive())
                    if (node.edges.ContainsKey(track))
                        if (node.edges[track].active)
                            tree.setActive(node, track);
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
            //Console.WriteLine(str);
            using (TextReader reader = new StringReader(str))
            {
                XMLCognitiveInterventionData result = (XMLCognitiveInterventionData)serializer.Deserialize(reader);
                return (result);
            }
        }

        /// <summary>
        /// Method for intervention trigger check
        /// </summary>
        internal void refresh()
        {
            CognitiveInterventionTree tree = getCognitiveInterventionTree();

            List<string> listOfActiveNodeIds = tree.getListOfActiveNodeIds();
            foreach (string nodeId in listOfActiveNodeIds)
            {
                CognitiveInterventionNode node = tree.getCognitiveInterventionNodeById(nodeId);
                node.isStillActive();
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
        #endregion Testmethods
    }

    /// <summary>
    /// Delegate - called with the intervention instance as argument
    /// </summary>
    /// <param name="interventionInstance"> intervention instance proposed by the asset</param>
    public delegate void CognitiveInterventionDelegate(string interventionType, string interventionInstance);

    /// <summary>
    /// Class managing the intervention activation.
    /// </summary>
    public class CognitiveInterventionTree
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

        public CognitiveInterventionTree(XMLCognitiveInterventionData data)
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
                newNode.timeToDeactivationInMiliSec = xmlNode.activationDuration;
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
        internal void deactivateNode(CognitiveInterventionNode nodeToDeactivate)
        {
            if (nodeToDeactivate.id.Equals(this.startingNode.id))
                return;
            CognitiveInterventionAsset.Handler.loggingCI("Deactivating Node '"+nodeToDeactivate.id+"'.");
            this.activeNodes.Remove(nodeToDeactivate);
        }

        /// <summary>
        /// Activates a given node.
        /// </summary>
        /// <param name="nodeToActivate"> Node which gets activated </param>
        internal void activateNode(CognitiveInterventionNode nodeToActivate)
        {
            CognitiveInterventionAsset.Handler.loggingCI("Activating Node '" + nodeToActivate.id + "'.");

            if (!this.activeNodes.Contains(nodeToActivate))
                this.activeNodes.Add(nodeToActivate);

            foreach (string trackIterator in nodeToActivate.edges.Keys)
                nodeToActivate.edges[trackIterator].active = true;

            nodeToActivate.setDeactivationTimestamp();

            //perform intervention ...
            if (nodeToActivate.edges.Count == 0)
            {
                //...if there is any to perform
                if (nodeToActivate.interventionType != null && nodeToActivate.interventionType != "")
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
            CognitiveInterventionAsset.Handler.loggingCI("New update: starting from Node '"+startingNode.id+"' because of track '"+track+"'.");

            CognitiveInterventionEdge edge = startingNode.edges[track];

            //set edge inactive, if not started from starting node
            if(!this.startingNode.id.Equals(startingNode.id))
                edge.active = false;

            //deactivate node if it is an exclusive edge
            if (edge.exclusive && !this.startingNode.id.Equals(startingNode.id))
            {
                if(!startingNode.wasActivatedThisUpdate)
                    deactivateNode(startingNode);
            }
            else
            {
                //deactivate node if all edges are deactivated
                bool anActiveEdgeExists = false;
                foreach (string trackIterator in startingNode.edges.Keys)
                    if (startingNode.edges[trackIterator].active)
                        anActiveEdgeExists = true;
                if (!anActiveEdgeExists && !this.startingNode.id.Equals(startingNode.id))
                    deactivateNode(startingNode);
            }

            //activate new edge
            activateNode(edge.successor);
            edge.successor.wasActivatedThisUpdate = true;
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
            CognitiveInterventionAsset.Handler.loggingCI(txt);
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
            if (!this.interventions.ContainsKey(interventionType))
                CognitiveInterventionAsset.Handler.loggingCI("Cant find the requested interventiontype '"+interventionType+"'!");

            List<string> interventionInstances = this.interventions[interventionType];
            

            //initialize instance counter, if not done yet
            if (interventionInstanceCounter == null)
            {
                interventionInstanceCounter = new Dictionary<string, int>();
                foreach(string interventionTypeString in this.interventions.Keys)
                {
                    foreach (string interventionInstanceString in this.interventions[interventionTypeString])
                    {
                        interventionInstanceCounter[interventionInstanceString] = 0;
                    }
                }
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

            CognitiveInterventionAsset.Handler.loggingCI("Proposed intervention-instance: " + chosenInstance);
            if(CognitiveInterventionAsset.Handler.cognitiveInterventionDelegate == null)
            {
                CognitiveInterventionAsset.Handler.loggingCI("There was no method defined (via the asset method 'setInterventionDelegate') for handling cognitive intervention events!", Severity.Error);
                throw new Exception("EXCEPTION: There was no method defined (via the asset method 'setInterventionDelegate') for handling cognitive intervention events!");
            }
            CognitiveInterventionAsset.Handler.cognitiveInterventionDelegate(interventionType, chosenInstance);
        }

        /* NOT TESTED!
        /// <summary>
        /// Method for creating a xml out of the structure
        /// </summary>
        /// <returns></returns>
        public string toXML()
        {
            string xml = "<cognitiveinterventiondata>";
            xml += "<cognitiveinterventiontypes>";
            foreach(string interventiontype in this.interventions.Keys)
            {
                xml += "<cognitiveinterventiontype>";
                xml += "<typeid>"+ interventiontype +"</typeid>";
                xml += "<cognitiveinterventioninstances>";
                foreach(string interventioninstance in this.interventions[interventiontype])
                {
                    xml += "<cognitiveinterventioninstance>";
                    xml += "<instance>"+ interventioninstance +"</instance>";
                    xml += "</cognitiveinterventioninstance>";
                }
                xml += "</cognitiveinterventioninstances>";
                xml += "</cognitiveinterventiontype>";
            }
            xml += "</cognitiveinterventiontypes>";
            xml += "<cognitiveinterventionstartingnode>" + this.startingNode.id +"</cognitiveinterventionstartingnode>";
            xml += "<cognitiveinterventionnodes>";
            foreach(CognitiveInterventionNode node in this.nodes)
            {
                xml += "<cognitiveinterventionnode>";
                xml += "<nodeid>"+ node.id +"</nodeid>";
                xml += "<activationDuration>"+ node.timeToDeactivationInMiliSec +"</activationDuration>";
                xml += "<interventiontype>"+node.interventionType+"</interventiontype>";
                xml += "<cognitiveinterventionedgelist>";
                foreach(KeyValuePair<string,CognitiveInterventionEdge> pair in node.edges)
                {
                    xml += "<cognitiveinterventionedge>";
                    xml += "<successorid>"+pair.Value.successor.id+"</successorid>";
                    xml += "<trackingid>"+pair.Key+"</trackingid>";
                    xml += "<type>exclusive</type>";
                    xml += "</cognitiveinterventionedge>";
                }
                xml += "</cognitiveinterventionedgelist>";
                xml += "</cognitiveinterventionnode>";
            }
            xml += "</cognitiveinterventionnodes>";

            return xml+ "</cognitiveinterventiondata>";
        }
        */

        #endregion Methods
    }

    /// <summary>
    /// Class representing a node in the cognitive intervention tree
    /// </summary>
    public class CognitiveInterventionNode
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

        /// <summary>
        /// time till deactivation of the node
        /// </summary>
        internal DateTime deactivationTimestamp;

        internal bool wasActivatedThisUpdate = false;

        /// <summary>
        /// Describes how long a node is active before deactivating again.
        /// </summary>
        internal int timeToDeactivationInMiliSec;

        #endregion Fields
        #region Constructors

        internal CognitiveInterventionNode(string id)
        {
            this.id = id;
        }

        #endregion Constructors
        #region Methods


        /// <summary>
        /// Method returning the current timestamp
        /// </summary>
        internal void setDeactivationTimestamp()
        {
            DateTime now = DateTime.Now;
            this.deactivationTimestamp = now.AddMilliseconds(timeToDeactivationInMiliSec);
        }

        /// <summary>
        /// Method checking if the node is still active
        /// </summary>
        /// <returns></returns>
        internal bool isStillActive()
        {
            //return true;
            
            if(DateTime.Now < deactivationTimestamp || CognitiveInterventionAsset.Handler.cognitiveInterventionTree.startingNode.id.Equals(this.id))
                return true;

            CognitiveInterventionAsset.Handler.loggingCI("Deactivating node '"+id+"' because too much time passed by without any relevant action.");
            //trigger intervention because of divergence to anticipated behaviour, if needed
            if(this.interventionType != null)
                triggerDerivationFromAnticipatedBehaviourIntervention();
            CognitiveInterventionAsset.Handler.cognitiveInterventionTree.deactivateNode(this);
            return false;
            
        }

        /// <summary>
        /// Method which get called, if the node gets deactivated because of too long waiting time (= derivation from anticipated behaviour)
        /// </summary>
        internal void triggerDerivationFromAnticipatedBehaviourIntervention()
        {
            if (this.interventionType != null && this.interventionType != "")
            {
                CognitiveInterventionAsset.Handler.loggingCI("Triggering intervention due to derivation from anticipated behaviour.");
                CognitiveInterventionAsset.Handler.cognitiveInterventionTree.performIntervention(this.interventionType);
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Class representing a edge in the cognitive intervention tree.
    /// </summary>
    public class CognitiveInterventionEdge
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
        [XmlElement("cognitiveinterventioninstances")]
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
        /// Duration for which the node gets activated
        /// </summary>
        [XmlElement("activationDuration")]
        public int activationDuration { get; set; }

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

        public XMLCognitiveInterventionNode() { }

        public XMLCognitiveInterventionNode(string id)
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

        public XMLCognitiveInterventionEdge() { }

        public XMLCognitiveInterventionEdge(string trackingId, string type, string successorId)
        {
            this.trackingId = trackingId;
            this.type = type;
            this.successorId = successorId;
        }

        #endregion Constructor
    }



    #endregion Serialization

}
