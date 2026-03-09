using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutSceneIntro : MonoBehaviour
{

    public Animator camAnimator;
    public GameObject WizardDialogue;
    public GameObject WizardText1;
    public GameObject WizardText2;
    public GameObject WizardText3;

    public GameObject FireBroImage;
    public GameObject LightBroImage;
    public GameObject FireText1;
    public GameObject FireText2;
    public GameObject LightText1;
    public GameObject LightText2;

    public GameObject EndScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camAnimator.Play("IntroSceneCam");
        StartCoroutine(WizardCoroutine());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator WizardCoroutine()
    {
       // Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(2f);
        WizardDialogue.SetActive(true);
        WizardText1.SetActive(true);
        yield return new WaitForSecondsRealtime(5);
        
        WizardText2.SetActive(true);
        yield return new WaitForSecondsRealtime(5);
        WizardText1.SetActive(false);
        WizardText2.SetActive(false);
        WizardText3.SetActive(true);
        yield return new WaitForSecondsRealtime(10);
        WizardText3 .SetActive(false);
        WizardDialogue.SetActive(false);
        camAnimator.SetBool("isNext", true);
        camAnimator.Play("IntroPanCam");

        StartCoroutine(MagicBrosCoroutine());
    }

    private IEnumerator MagicBrosCoroutine()
    {
        //Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(2);
        FireBroImage.SetActive(true);
        FireText1.SetActive(true);
        yield return new WaitForSecondsRealtime(2);
        LightBroImage.SetActive(true) ;
        LightText1.SetActive(true);
        yield return new WaitForSecondsRealtime(4);
        FireText1 .SetActive(false) ;
        LightText1.SetActive(false) ;
        
        FireText2 .SetActive(true) ;
        yield return new WaitForSecondsRealtime(1) ;
        LightText2.SetActive(true) ;
        yield return new WaitForSecondsRealtime(5);
        FireText2 .SetActive(false) ;
        LightText2 .SetActive(false) ;
        LightBroImage.SetActive (false) ;
        FireBroImage .SetActive (false) ;
        EndScreen.SetActive(true);
        yield return new WaitForSecondsRealtime(5);
        SceneManager.LoadScene("IntroLevel");

    }
  
}
