using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using System; 
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.Networking;

namespace BuildLesson
{
    public class TouchHandler : MonoBehaviour
    {
        private static TouchHandler instance; 
        public static TouchHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TouchHandler>();
                }
                return instance; 
            }
        }
        public static event Action onResetStatusFeature; 
        public static event Action<GameObject> onSelectChildObject; 

        public GameObject UIComponent;
        const float ROTATION_RATE = 0.08f;
        const float LONG_TOUCH_THRESHOLD = 1f; 
        const float ROTATION_SPEED = 0.5f; 
        float touchDuration = 0.0f; 
        Touch touch; 
        Touch touchZero; 
        Touch touchOne; 
        float originDelta; 
        Vector3 originScale;

        Vector3 originLabelScale = new Vector3(1f, 1f, 1f);
        Vector3 originLabelTagScale = new Vector3(7f, 1f, 1f);
        Vector3 originLineScale = new Vector3(1f, 1f, 1f); 
        Vector3 originScaleSelected;
        bool isMovingByLongTouch = false; 
        bool isLongTouch = false;
        float currentDelta;
        float scaleFactor;

        private GameObject currentSelectedObject; 
        private GameObject recentSelectedObject;
        private Vector3 centerPosition;
        private Vector3 mOffset; 
        private float mZCoord;

        // private bool isLabelOnEdit = false;
        private string currentSelectedLabelOrganName;

        private Vector3 hitPoint;
        private Vector2 hitPoint2D;

        // Panel DeleteTag
        public GameObject panelPopUpDeleteLabel;
        public Button btnExitPopupDeleteLabel; 
        public Button btnDeleteLabel; 
        public Button btnCancelDeleteLabel;

        // Panel AddActivities: addVideo, addAudio
        public GameObject panelAddActivities;
        public Button btnAddAudioLabel;
        public Button btnAddVideoLabel;
        public Button btnCancelAddActivities;

        private int calculatedSize = 20;

        // Make just one instance of labelEditObject, throught the flow, hide and show it also change it's text 
        GameObject labelEditObject;

        void Start()
        {
            InitUILabel();
        }
        void InitUILabel()
        {
            // Button EditLabel
            labelEditObject = Instantiate(Resources.Load(PathConfig.MODEL_TAG_EDIT) as GameObject);
            // Add functional to the buttons of labelEditObject
            labelEditObject.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() => 
            {
                InputField inpField = labelEditObject.transform.GetChild(0).gameObject.GetComponent<InputField>();
                inpField.ActivateInputField();
                inpField.Select();
                inpField.onEndEdit.AddListener(delegate{OnEndEditLabel(inpField);});
            });
            // Button DeleteLabel
            labelEditObject.transform.GetChild(4).gameObject.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(HandlerDeleteTag(1)));
            btnExitPopupDeleteLabel.onClick.AddListener(ExitPopupDeleteLabel);
            btnCancelDeleteLabel.onClick.AddListener(ExitPopupDeleteLabel);
            // Button AddLabel: addAudio, addVideo for Label
            labelEditObject.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(HandlerAddTag()));
            btnAddAudioLabel.onClick.AddListener(AddAudioLabel);
            btnAddVideoLabel.onClick.AddListener(AddVideoLabel);
            btnCancelAddActivities.onClick.AddListener(CancelAddActivities);
        }

        private string formatString (string inputString, int maxSize)
        {
            if (inputString.Length > maxSize)
            {
                return inputString.Remove(maxSize) + "...";
            }
            return inputString;
        }
        
        // Todo: May be LabelEdit2D should Instantiate only one time! During the process, then we just setActive false and reset the index
        public void HandleTouchInteraction()    
        {
            if (ObjectManager.Instance.CurrentObject == null)
            {
                return;
            }
            if (Input.touchCount == 1)
            {
                touch = Input.GetTouch(0); 
                if (touch.tapCount == 1)
                {
                    HandleSingleTouch(touch);
                }
                else if (touch.tapCount == 2)
                {
                    touch = Input.touches[0];
                    if (touch.phase == TouchPhase.Ended)
                    {
                        HandleDoupleTouch(touch);
                    }
                }
            }
            // Add HandleSimultaneousThreeTouch
            if (Input.touchCount == 3)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved && Input.GetTouch(2).phase == TouchPhase.Moved)
                {
                    HandleSimultaneousThreeTouch(Input.GetTouch(1));
                }
            }
            // Add HandleSimultaneousTouch
            else if (Input.touchCount == 2)
            {
                touchZero = Input.GetTouch(0);
                touchOne = Input.GetTouch(1);
                HandleSimultaneousTouch(touchOne, touchZero);
            }
        }

        // Todo: Rewrite all logit code inside this function
        private void HandleSingleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began: 
                {
                    // Check whether we are in the edit mode of a label 
                    if (LabelManager.Instance.IsLabelOnEdit)
                    { 
                        // we are in the edit mode
                        Debug.Log("Check: " + currentSelectedObject);
                        if (currentSelectedObject != null)
                        {
                            // this is when the first time the Normal Label as well as the 2D edit Label created 
                            // Change the state to the normal, also reset the currentSelectedLabel
                            LabelManager.Instance.IsLabelOnEdit = !LabelManager.Instance.IsLabelOnEdit;
                            // Delete the current edited label, and reset the index ref inside TagHandle
                            TagHandler.Instance.ResetEditLabelIndex(); // Set to -1 auto not update, so I don't care about the labelEditTags
                            // Destroy(TagHandler.Instance.labelEditTag);
                            TagHandler.Instance.labelEditTag.SetActive(false);
                        }

                        // Todo: Unused code, may be resuse part of code 
                        // else
                        // {
                        //     Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        //     RaycastHit hit; 
                        //     if (Physics.Raycast(ray, out hit))
                        //     {
                        //         Debug.Log("Hit: " + hit.transform.gameObject.name);
                        //         if (hit.transform.gameObject.tag == TagConfig.labelModelEdit)
                        //         {
                        //             Debug.Log("Hit the edit label: "); 
                        //             LabelManager.Instance.IsLabelOnEdit = !LabelManager.Instance.IsLabelOnEdit; // Edit ve thuong 
                        //             hit.transform.parent.parent.GetChild(2).gameObject.SetActive(true); // Label thuong 
                        //             hit.transform.parent.gameObject.SetActive(false); // label edit 
                        //         }
                        //         else if (hit.transform.gameObject.name == "BtnEdit")
                        //         {
                        //             // Forcus promatically on InputField click
                        //             // hit.transform.
                        //         }
                        //     }
                        // }
                    }

                    if (!LabelManager.Instance.IsLabelOnEdit)
                    {
                        // Handle case when the label is normal. There are 2 cases: 
                        // 1. When hitting gameObject for create a new Label 
                        // 2. When hitting the Label for edit

                        // Touching when the label is normal 
                        Debug.Log("Hit: IsLabelOnEdit" + LabelManager.Instance.IsLabelOnEdit);
                        // Construct a ray from the current touch coordinates
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        RaycastHit hit; 
                        if (Physics.Raycast(ray, out hit))
                        {
                            Debug.Log("Hit: " + hit.transform.gameObject.name);
                            if (hit.transform.gameObject.tag == TagConfig.labelModel)
                            {
                                Debug.Log("Hit the Normal label: "); // Normal mode + hit the label, then recreate the 2dLabel, make it displayed and add it into the TagHandler class 
                                // Get the text inside the hit object 
                                string hitLabelText = hit.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text;
                                // LabelManager.Instance.IsLabelOnEdit = !LabelManager.Instance.IsLabelOnEdit;
                                
                                // Reinit the labe2dEdit, as I mentioned in the todo, must created the gameObject one time
                                // GameObject labelEditObject = Instantiate(Resources.Load(PathConfig.MODEL_TAG_EDIT) as GameObject);
                        
                                // Add functional to the buttons of labelEditObject
                                // labelEditObject.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() => 
                                // {
                                //     InputField inpField = labelEditObject.transform.GetChild(0).gameObject.GetComponent<InputField>();
                                //     inpField.ActivateInputField();
                                //     inpField.Select();
                                // });

                                // Add the label into the TagHandle
                                // labelEditObject.transform.GetChild(0).GetComponent<InputField>().text = hitLabelText; 
                                TagHandler.Instance.labelEditTag.transform.GetChild(0).GetComponent<InputField>().text = hitLabelText;
                                TagHandler.Instance.updateCurrentEditingIdx(hitLabelText);
                                TagHandler.Instance.labelEditTag.SetActive(true);

                                // Todo: Limitation here, change to singleton init will resolve this problem 
                                // labelEditObject.transform.parent = UIComponent.transform;
                                // labelEditObject.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                                // TagHandler.Instance.labelEditTag = labelEditObject;
                                // TagHandler.Instance.updateCurrentEditingIdx(hitLabelText);
                            }
                        }
                    }
                    // Consider
                    var rs = GetChildOrganOnTouchByTag(touch.position);
                    currentSelectedObject = rs.Item1; 
                    recentSelectedObject = rs.Item1;
                    hitPoint = rs.Item2;
                    isMovingByLongTouch = currentSelectedObject != null; 
                    break; 
                }
                case TouchPhase.Stationary: 
                {
                    Debug.Log("Stationary state");
                    Debug.Log("isMovingByLongTouch: " + isMovingByLongTouch + "!isLongTouch" + !isLongTouch);
                    if (isMovingByLongTouch && !isLongTouch)
                    {
                        touchDuration += Time.deltaTime;
                        if (touchDuration > LONG_TOUCH_THRESHOLD)
                        {
                            OnLongTouchInvoke();
                            Debug.Log("OnLongTouchInvoke");
                        }
                    }
                    break;
                }
                case TouchPhase.Moved:
                {
                    Debug.Log("Touch phase move: ");
                    if (isLongTouch)
                    {
                        // Drag(touch, currentSelectedObject);
                    }
                    else
                    {
                        Rotate(touch);
                    }
                    break;
                }
                case TouchPhase.Ended: 
                {
                    ResetLongTouch(); 
                    break;
                }
                case TouchPhase.Canceled: 
                {
                    ResetLongTouch(); 
                    break;
                }
            }
        }
        private Vector3 GetTouchPositionAsWorldPoint(Touch touch)
        {
            Vector3 touchPoint = touch.position;
            touchPoint.z = mZCoord;
            return Camera.main.ScreenToWorldPoint(touchPoint);
        }

        private void Rotate(Touch touch)
        {
            if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_AR)
            {
                ObjectManager.Instance.OriginObject.transform.rotation *= Quaternion.Euler(new Vector3(0, -touch.deltaPosition.x * ROTATION_SPEED, 0));
            }
            else if (ModeManager.Instance.Mode == ModeManager.MODE_EXPERIENCE.MODE_3D)
            {
                ObjectManager.Instance.OriginObject.transform.Rotate(touch.deltaPosition.y * ROTATION_RATE, -touch.deltaPosition.x * ROTATION_RATE, 0, Space.World);
            }
        }

        void OnLongTouchInvoke()
        {
            if (currentSelectedObject != null)
            {
                Debug.Log("Current selected object: " + currentSelectedObject.name);
                // Project 3D point into the camera image plane
                hitPoint2D = Camera.main.WorldToScreenPoint(hitPoint);
                // Create label 2D
                GameObject label2D = Instantiate(Resources.Load(PathConfig.MODEL_TAG_CONFIG) as GameObject);
                label2D.tag = "Tag2D";
                label2D.transform.parent = UIComponent.transform;
                label2D.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
                label2D.transform.position = hitPoint2D;
                label2D.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() => onCreatedLabel(label2D));
                // LabelManager.Instance.CreateLabel(currentSelectedObject, hitPoint); 
            }
            isLongTouch = true;
        }

        void ResetLongTouch()
        {
            touchDuration = 0f;
            isLongTouch = false;
            isMovingByLongTouch = false;
            currentSelectedObject = null;
        }

        private void Drag(Touch touch, GameObject obj)
        {
            if (obj != null)
            {
                obj.transform.position = GetTouchPositionAsWorldPoint(touch) + mOffset;
            }
        }

        IEnumerator HightLightObject()
        {
            originScaleSelected = currentSelectedObject.transform.localScale;
            currentSelectedObject.transform.localScale = originScaleSelected * 1.5f;
            yield return new WaitForSeconds(0.12f);
            currentSelectedObject.transform.localScale = originScaleSelected;
        }

        private (GameObject, Vector3) GetChildOrganOnTouchByTag(Vector3 position)
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            RaycastHit hit; 
            if (Physics.Raycast(ray, out hit))
            {
                // .root =))) 
                // if (hit.collider.transform.parent.gameObject.tag == TagConfig.ORGAN_TAG)
                if (hit.collider.transform.root.gameObject.tag == TagConfig.ORGAN_TAG)
                {
                    if (hit.collider.transform.parent == ObjectManager.Instance.CurrentObject.transform)
                    {
                        return (hit.collider.gameObject, hit.point);
                    }
                }
            }
            return (null, new Vector3(0f, 0f, 0f));
        }

        private void HandleDoupleTouch(Touch touch)
        {
            GameObject selectedObject = Helper.GetChildOrganOnTouchByTag(touch.position);
            if (selectedObject == null || selectedObject == ObjectManager.Instance.OriginObject || ObjectManager.Instance.CurrentObject.transform.childCount < 1)
            {
                return;
            }
            onSelectChildObject?.Invoke(selectedObject);
        }

        void onCreatedLabel(GameObject destroyedObj)
        {
            // Sphere red 
            GameObject s;
            s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var spereRenderer = s.GetComponent<Renderer>(); 
            spereRenderer.material.SetColor("_Color", Color.red);
            Debug.Log("Current selected object: " + recentSelectedObject.name);
            s.transform.parent = recentSelectedObject.transform;
            s.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
            s.transform.position = hitPoint; // Global variable

            LabelManager.Instance.IsLabelOnEdit = !LabelManager.Instance.IsLabelOnEdit;

            GameObject labelObject = Instantiate(Resources.Load(PathConfig.MODEL_TAG) as GameObject); // Normal label 
            labelObject.transform.localScale *=  ObjectManager.Instance.OriginScale.x / ObjectManager.Instance.OriginObject.transform.localScale.x ;
            labelObject.transform.SetParent(recentSelectedObject.transform, false);
            centerPosition = CalculateCentroid(ObjectManager.Instance.OriginObject);
            currentSelectedLabelOrganName = destroyedObj.transform.GetChild(0).gameObject.GetComponent<InputField>().text;

            // Add functional to the buttons of labelAddDeleteObject
            // labelEditObject.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(HandlerDeleteTag(labelId)));
            // Add functional to the buttons of labelDeleteTagObject 
            // All stuffs handled in SetLabel function
            SetLabel(currentSelectedLabelOrganName, hitPoint, recentSelectedObject, ObjectManager.Instance.OriginObject, centerPosition, labelObject, labelEditObject);
            Debug.Log("Hit point 3D in world pos: " + hitPoint.x + ", " + hitPoint.y + ", " + hitPoint.z);
            Destroy(destroyedObj);

            // Save label info to the server
            // Prepare data 
            string level = LabelManager.Instance.getIndexGivenGameObject(ObjectManager.Instance.OriginObject, recentSelectedObject);
            StartCoroutine(LabelManager.Instance.SaveCoordinate(SaveLesson.lessonId, ModelStoreManager.modelId, currentSelectedLabelOrganName, hitPoint, level));
        }

        void OnEndEditLabel(InputField input)
        {
            if (input.text.Length > 0) 
            {
                Debug.Log("Text has been entered: " + input.text);
                // Change the text inside the corresponding counter part
                TagHandler.Instance.addedTags[TagHandler.Instance.currentEditingIdx].transform.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshPro>().text = input.text;
            }
        }

        private void SetLabel(string name, Vector3 hitpoint, GameObject currentObject, GameObject parentObject, Vector3 rootPosition, GameObject label, GameObject editLabel)
        {
            GameObject line = label.transform.GetChild(0).gameObject; 
            GameObject labelName = label.transform.GetChild(1).gameObject;  
            // labelName.transform.GetChild(0).GetComponent<TextMeshPro>().text = name; // text for 3d label
            labelName.transform.GetChild(0).GetComponent<TextMeshPro>().text = formatString(name, calculatedSize);
            
            Bounds parentBounds = GetParentBound(parentObject, rootPosition);
            Bounds objectBounds = currentObject.GetComponent<Renderer>().bounds;
            // localPosition: pos w.r.t parent 
            // dir: in the Brain's coordinate system 
            // Vector3 dir = currentObject.transform.localPosition - rootPosition; 

            // Convert hitPoint - in world system to the Brain's coordinate system
            // Vector3 dir = hitPoint - rootPosition; 
            Vector3 dir = parentObject.transform.InverseTransformPoint(hitPoint) - rootPosition;           
            labelName.transform.localPosition = 1f / parentObject.transform.localScale.x * parentBounds.max.magnitude * dir.normalized;
            // labelName.transform.localPosition = 2f * parentBounds.max.magnitude * dir.normalized;
            
            line.GetComponent<LineRenderer>().useWorldSpace = false;
            line.GetComponent<LineRenderer>().widthMultiplier = 0.25f * parentObject.transform.localScale.x;  // 0.2 -> 0.05 then 0.02 -> 0.005
            line.GetComponent<LineRenderer>().SetVertexCount(2);
            line.GetComponent<LineRenderer>().SetPosition(0, currentObject.transform.InverseTransformPoint(hitPoint));
            line.GetComponent<LineRenderer>().SetPosition(1, labelName.transform.localPosition);
            line.GetComponent<LineRenderer>().SetColors(Color.black, Color.black);

            // Set position and the text inside editLabel too.
            editLabel.transform.GetChild(0).GetComponent<InputField>().text = name;
            Vector2 endPoint = Camera.main.WorldToScreenPoint(labelName.transform.position);

            editLabel.transform.parent = UIComponent.transform;
            editLabel.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
            editLabel.transform.position = endPoint;
            // Update TagHandler
            // Add normal Label into the TagHander 
            TagHandler.Instance.AddTag(label);
            // Also update the counterpart
            if (TagHandler.Instance.labelEditTag == null) 
            {
                TagHandler.Instance.labelEditTag = editLabel;
            }  
        }

        // Handle DeleteTag
        IEnumerator HandlerDeleteTag(int labelId)
        {
            panelPopUpDeleteLabel.SetActive(true);
            //  Yes / No 
            // Listener to Yes -> Function (delete)/ No -> ()
            yield return null;
            // Debug.Log("Trigger delete label: ");
            // UnityWebRequest webRequest = UnityWebRequest.Delete(String.Format(APIUrlConfig.DeleteLabel, labelId));
            // webRequest.SetRequestHeader("Authorization", PlayerPrefs.GetString("user_token"));
            // yield return webRequest.SendWebRequest();
            // if (webRequest.isNetworkError || webRequest.isHttpError)
            // {
            //     Debug.Log("An error has occur");
            //     Debug.Log(webRequest.error);
            // }
            // else
            // {
            //     if (webRequest.isDone)
            //     {
            //         Debug.Log("Delete audio: ");
            //     }
            // }
        }
        void ExitPopupDeleteLabel()
        {
            panelPopUpDeleteLabel.SetActive(false);
        }

        // Handle AddTag: addAudio, addVideo
        IEnumerator HandlerAddTag()
        {   
            panelAddActivities.SetActive(true);
            Debug.Log("Handler Add Tag: ");
            yield return null;
        }
        void AddAudioLabel()
        {
            
        }
        void AddVideoLabel()
        {
            
        }
        void CancelAddActivities()
        {
            panelAddActivities.SetActive(false);
        }
        private Vector3 CalculateCentroid(GameObject obj)
        {
            Transform[] children;
            Vector3 centroid = new Vector3(0, 0, 0);
            children = obj.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if(child != obj.transform)
                {
                    centroid += child.transform.position;
                }  
            }
            centroid /= (children.Length - 1);
            return centroid;
        }

        private Bounds GetParentBound(GameObject parentObject, Vector3 center)
        {
            foreach (Transform child in parentObject.transform)
            {
                center += child.gameObject.GetComponent<Renderer>().bounds.center;
            }
            center /= parentObject.transform.childCount;
            Bounds bounds = new Bounds(center, Vector3.zero);
            foreach(Transform child in parentObject.transform)
            {
                bounds.Encapsulate(child.gameObject.GetComponent<Renderer>().bounds);
            }
            return bounds;
        }

        // Zoom in, zoom out
        private void HandleSimultaneousTouch(Touch touchZero, Touch touchOne)
        {
            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                originDelta = Vector2.Distance(touchZero.position, touchOne.position);
                originScale = ObjectManager.Instance.OriginObject.transform.localScale;
            }
            else if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
            {
                currentDelta = Vector2.Distance(touchZero.position, touchOne.position);
                scaleFactor = currentDelta / originDelta;
                ObjectManager.Instance.OriginObject.transform.localScale = originScale * scaleFactor;
                Debug.Log("Scale factor: " + scaleFactor);
            }
        }

        private void HandleSimultaneousThreeTouch(Touch touch)
        {
            Drag(touch, ObjectManager.Instance.OriginObject);
        }
    }
}

