using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using System.Reflection;
using System.Runtime.Versioning;

namespace BuildLesson
{
    public class BuildLessonManager : MonoBehaviour
    {
        public Button btnLabel;
        public Button btnSeparate;
        public Button btnXray;
        public Button btnAdd;
        public Animator toggleListItemAnimator;
        public GameObject record; 
        public GameObject saveRecord; 
        public GameObject addVideo; 
        public GameObject upload; 
        public GameObject addAudio; 
        public GameObject edit; 
        private GameObject label2D;
        void Start()
        {
            ObjectManager.Instance.InitOriginalExperience();
            InitInteractions();
            InitEvents();
        }
        void Update()
        {
            // Check whether the pannel is opened 
            Debug.Log("Value of the animator: " + toggleListItemAnimator.GetBool(AnimatorConfig.isShowMeetingMemberList));
            label2D = GameObject.FindWithTag("Tag2D");
            Debug.Log("Label 2d: " + label2D);
            if (!toggleListItemAnimator.GetBool(AnimatorConfig.isShowMeetingMemberList) && 
                !record.activeSelf && 
                !saveRecord.activeSelf && 
                !addVideo.activeSelf && 
                !upload.activeSelf && 
                !addAudio.activeSelf && 
                !edit.activeSelf && label2D == null)
            {
                TouchHandler.Instance.HandleTouchInteraction();
            }
           
            EnableFeature();
        }
        void EnableFeature()
        {
            if (ObjectManager.Instance.CurrentObject != null)
            {
                if (ObjectManager.Instance.CurrentObject.transform.childCount == 0)
                {
                    btnLabel.interactable = false;
                    btnSeparate.interactable = false;
                }
                else
                {
                    btnLabel.interactable = true;
                    btnSeparate.interactable = true;
                }
            }
        }
        void OnEnable()
        {
            TouchHandler.onSelectChildObject += OnSelectChildObject;
            TreeNodeManager.onClickNodeTree += OnClickNodeTree;
            ObjectManager.onResetObject += OnResetObject;
        }
        void OnDisable()
        {
            TouchHandler.onSelectChildObject -= OnSelectChildObject;
            TreeNodeManager.onClickNodeTree -= OnClickNodeTree; 
            ObjectManager.onResetObject -= OnResetObject;
        }
        void OnResetObject()
        {
            // ObjectManager.Instance.DestroyOriginalObject();
            ObjectManager.Instance.InitOriginalExperience();
            TreeNodeManager.Instance.ClearAllNodeTree();
            XRayManager.Instance.HandleXRayView(XRayManager.Instance.IsMakingXRay);
            Helper.ResetStatusFeature();
        }
        void OnSelectChildObject(GameObject selectedObject)
        {
            OnResetStatusFeature();
            TreeNodeManager.Instance.DisplaySelectedObject(selectedObject, ObjectManager.Instance.CurrentObject);
            ObjectManager.Instance.ChangeCurrentObject(selectedObject);
            TreeNodeManager.Instance.CreateChildNodeUI(selectedObject.name);
        }
        void OnClickNodeTree(string nodeName)
        {
            OnResetStatusFeature();
            XRayManager.Instance.HandleXRayView(false);
            if (nodeName != ObjectManager.Instance.CurrentObject.name)
            {
                GameObject selectedObject = GameObject.Find(nodeName);
                TreeNodeManager.Instance.DisplayAllChildSelectedObject(selectedObject);
                // Conjoined
                ObjectManager.Instance.CurrentObject = selectedObject;
                SeparateManager.Instance.HandleSeparate(false);

                ObjectManager.Instance.ChangeCurrentObject(selectedObject);
                TreeNodeManager.Instance.RemoveItem(nodeName);
                // StartCoroutine(Helper.MoveObject(Camera.main.gameObject, Camera.main.transform.position));
            }
            ObjectManager.Instance.OriginObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        void OnResetStatusFeature()
        {
            LabelManager.Instance.IsShowingLabel = false;
            LabelManager.Instance.HandleLabelView(LabelManager.Instance.IsShowingLabel);

            SeparateManager.Instance.IsSeparating = false;
            SeparateManager.Instance.HandleSeparate(SeparateManager.Instance.IsSeparating);
        }

        void InitInteractions()
        {
            LabelManager.Instance.IsShowingLabel = true;
        }
        void InitEvents()
        {
            btnAdd.onClick.AddListener(ToggleMenuAdd); 
            btnLabel.onClick.AddListener(HandleLabelView);
            btnSeparate.onClick.AddListener(HandleSeparation); 
            btnXray.onClick.AddListener(HandleXRayView);
            // btnShowGuideBoard.onClick.AddListener(HandleGuideBoard);
            // btnExitGuideBoard.onClick.AddListener(HandleGuideBoard); 
        }
        void ToggleMenuAdd()
        {
            PopUpBuildLessonManager.Instance.IsClickedAdd = !PopUpBuildLessonManager.Instance.IsClickedAdd;
            PopUpBuildLessonManager.Instance.ShowListAdd(PopUpBuildLessonManager.Instance.IsClickedAdd);
        }
        void HandleLabelView()
        {
            LabelManager.Instance.IsShowingLabel = !LabelManager.Instance.IsShowingLabel;
            LabelManager.Instance.HandleLabelView(LabelManager.Instance.IsShowingLabel);
        }
        void HandleSeparation()
        {
            SeparateManager.Instance.IsSeparating = !SeparateManager.Instance.IsSeparating;
            SeparateManager.Instance.HandleSeparate(SeparateManager.Instance.IsSeparating);
        }
        void HandleXRayView()
        {
            XRayManager.Instance.IsMakingXRay = !XRayManager.Instance.IsMakingXRay;
            XRayManager.Instance.HandleXRayView(XRayManager.Instance.IsMakingXRay);
        }

        // call API CreateModelLabel
        public IEnumerator SaveLabelLesson()
        {
            var webRequest = new UnityWebRequest(APIUrlConfig.CreateModelLabel, "POST");

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("AAA");
            webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", PlayerPrefs.GetString("user_token"));
            
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                // Invoke error action
                Debug.Log("an error has occured"); 
                Debug.Log(webRequest.error);
            }
            else
            {
                // check when response is received
                if (webRequest.isDone)
                {

                }
            }
        }
    }
}
