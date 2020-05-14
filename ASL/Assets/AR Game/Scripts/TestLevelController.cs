using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleDemos
{
    /// <summary>
    /// Demonstrates how you can use the SetWorldOrigin parameter when creating a cloud anchor to synchronize AR origins.
    /// </summary>
    public class TestLevelController : MonoBehaviour
    {
        /// <summary>Singleton for this class so that functions can be called after objects and cloud anchors are created using the same parameter they were created with</summary>
        private static TestLevelController m_Controller;

        /// <summary>Determines which object to spawn: Cloud Anchor or normal sphere</summary>
        public Dropdown m_ObjectToSpawnDropDown;

        /// <summary>Text that displays scene information to the user</summary>
        public Text m_DisplayInformation;

        /// <summary>GameObject that represents the world origin cloud anchor (will be placed on where the world origin is) </summary>
        public GameObject m_WorldOriginCloudAnchorObject;

        /// <summary> GameObject that represents a cloud anchor object (will be placed where the cloud anchor is)</summary>
        public GameObject m_NormalCloudAnchorObject;

        /// <summary>Flag indicating if the world origin has been initialized or not - it should only be set once</summary>
        private bool m_WorldOriginInitilized = false;

        /// <summary> Gets the hit position where the user touched the screen to help record where the object is verses where the user tapped</summary>
        private Pose? m_LastValidPose;

        /// <summary>Text that displays scene information to the user</summary>
        public Text m_CoinsCollected;
        
        /// <summary>Text that displays scene information to the user</summary>
        /// public Text m_Timer;
        
        /// toggle for enable/disable raycasting
        public Toggle m_Toggle;
        /// <summary>Called before start - sets up the singleton object for this class</summary>
        private void Awake()
        {
            m_Controller = this;
        }

        /// <summary>Called on start up - sets the initial text for the user</summary>
        void Start()
        {
            m_DisplayInformation.text = "The first location you touch will spawn the World Origin Cloud Anchor. " +
                "Only 1 player can spawn this cloud anchor and it should always be the first cloud anchor created if you plan on utilizing cloud anchors.";
                
            /// Set score text
            m_CoinsCollected.text="Score : " + GameVariables.collectCoins;
            
            /// Add event listener
            m_Toggle = GetComponent<Toggle>();
            //Add listener for when the state of the Toggle changes, to take action
            m_Toggle.onValueChanged.AddListener(delegate {
                ToggleValueChanged(m_Toggle);
            });
            GameVariables.isRayCasting = m_Toggle.isOn;


        }

        /// <summary> The logic of this example - listens for screen touches and spawns whichever object is currently active on the drop down menu</summary>
        void Update()
        {
            //m_Timer.text=GameVariables.debugStr;

            m_CoinsCollected.text="Score : " + GameVariables.collectCoins;
            
            Pose? touchPose = GetTouch();
            if (touchPose == null) //If we didn't hit anything - return
            {
                return;
            }

            //If we haven't set the world origin yet and we are player 1. By checking if we are playing 1 it 
            //helps ensure that two people don't attempt to set the World Origin at the same time
            if (!m_WorldOriginInitilized && ASL.GameLiftManager.GetInstance().AmLowestPeer())
            {
                m_WorldOriginInitilized = true;
                m_DisplayInformation.text = "Creating World Origin Visualization object now.";

                m_LastValidPose = touchPose;
                //It doesn't matter what we set the position to be to when creating this object because it will be reset to zero before it gets parented to its cloud anchor. 
                //Setting it to 100 initially just helps prevent confusion as we shouldn't see the world origin cloud anchor object until the world origin cloud anchor is set
                ASL.ASLHelper.InstanitateASLObject("Level", new Vector3(100, 100, 100), Quaternion.identity, string.Empty, string.Empty, SpawnWorldOrigin);

            }
        }

        /// <summary>
        /// Gets the location of the user's touch
        /// </summary>
        /// <returns>Returns null if UI or nothing touched</returns>
        private Pose? GetTouch()
        {
            
            Touch touch;
            // If the player has not touched the screen then the update is complete.
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return null;
            }

            // Ignore the touch if it's pointing on UI objects.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return null;
            }

            return ASL.ARWorldOriginHelper.GetInstance().Raycast(Input.GetTouch(0).position);
        }

        /// <summary>
        /// Spawns the world origin cloud anchor after the world origin object visualizer has been created (blue cube)
        /// </summary>
        /// <param name="_worldOriginVisualizationObject">The game object that represents the world origin</param>
        public static void SpawnWorldOrigin(GameObject _worldOriginVisualizationObject)
        {
            m_Controller.m_DisplayInformation.text = "Creating World Origin Cloud Anchor now.";
            _worldOriginVisualizationObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                //_worldOriginVisualizationObject will be parented to the cloud anchor which is the world origin, thus showing where the world origin is
                ASL.ASLHelper.CreateARCoreCloudAnchor(m_Controller.m_LastValidPose, _worldOriginVisualizationObject.GetComponent<ASL.ASLObject>(), WorldOriginTextUpdate, false, true);
            });
        }

        /// <summary>
        /// The cloud anchor callback that informs the user the world origin is finished being created
        /// </summary>
        /// <param name="_worldOriginVisualizationObject">The game object that represents the world origin</param>
        /// <param name="_spawnLocation">The pose of the world origin</param>
        public static void WorldOriginTextUpdate(GameObject _worldOriginVisualizationObject, Pose _spawnLocation)
        {
            m_Controller.m_DisplayInformation.text = "Finished creating World Origin Cloud Anchor. You are now free to create more cloud anchors or objects. " +
                "The position you touched on the screen was: " + _spawnLocation.position + " and the anchor's world position is: " + _worldOriginVisualizationObject.transform.position
                + " with a local position of: " + _worldOriginVisualizationObject.transform.localPosition;
                m_Controller.m_DisplayInformation.gameObject.SetActive(false);
                
                /// Start time counter 
                GameVariables.gameStarted = true;
        }
        
        //Output the new state of the Toggle into Text
        void ToggleValueChanged(Toggle change)
        {
            GameVariables.isRayCasting = m_Toggle.isOn;
        }

    }
}