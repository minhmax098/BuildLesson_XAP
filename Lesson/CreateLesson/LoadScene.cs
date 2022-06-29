using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using System; 
using UnityEngine.SceneManagement; 
using UnityEngine.Networking; 
using System.Text;

namespace CreateLesson
{
    public class LoadScene : MonoBehaviour
    {
        public GameObject spinner;
        public List3DModel[] myData;
        public List3DModel currentModel; 
        public GameObject bodyObject; 
        public Button buildLessonBtn;
        public Button cancelBtn;
        private ListOrgans listOrgans;
        public GameObject dropdownObj; 
        private Dropdown dropdown;

        private List<Dropdown.OptionData> option_ = new List<Dropdown.OptionData>();
        void Start()
        {
            Screen.orientation = ScreenOrientation.Portrait; 
            StatusBarManager.statusBarState = StatusBarManager.States.TranslucentOverContent;
            StatusBarManager.navigationBarState = StatusBarManager.States.Hidden;
            buildLessonBtn.onClick.AddListener(CreateLessonInfo);
            // myData = LoadData.Instance.GetLessonByID(LessonManager.lessonId.ToString()).data; 
            // currentModel = myData[0];
            // StartCoroutine(LoadCurrentModel(currentModel));
            spinner.SetActive(false);
            dropdown = dropdownObj.GetComponent<Dropdown>();
            updateDropDown();
        }
        
        void updateDropDown()
        {
            listOrgans = LoadData.Instance.getListOrgans();
            foreach (ListOrganLesson organ in listOrgans.data)
            {
                option_.Add(new Dropdown.OptionData(organ.organsName));
            }
            dropdown.AddOptions(option_);
            dropdown.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("--Choose--"));
            dropdown.GetComponent<Dropdown>().value = dropdown.GetComponent<Dropdown>().options.Count - 1; 
        }

        public void CreateLessonInfo() 
        {   
            PublicLesson newLesson = new PublicLesson();
            newLesson.modelId = ModelStoreManager.modelId;
            newLesson.lessonTitle = bodyObject.transform.GetChild(0).GetChild(1).GetComponent<InputField>().text;
            newLesson.organId = listOrgans.data[dropdown.value].organsId;
            Debug.Log("ORGAN ID: "+ newLesson.organId);
            newLesson.lessonObjectives = bodyObject.transform.GetChild(2).GetChild(1).GetComponent<InputField>().text;
            newLesson.publicLesson = 1;  // To do: Get from UI
            
            StartCoroutine(LoadData.Instance.buildLesson(newLesson));
        }
    }
}
