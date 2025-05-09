using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject[] tutorialSlides;
    [SerializeField] private TestManager testManager;

    private int  currentSlideIndex = 0;

    private void Start()
    {
        tutorialCanvas = this.gameObject;
        tutorialCanvas.SetActive(true);

        // set first slide active only
        for (int i = 0; i < tutorialSlides.Length; i++)
        {
            tutorialSlides[i].SetActive(i == 0);
        }

        SetupXRInteractables();
    }

    private void SetupXRInteractables()
    {
        for (int i = 0; i < tutorialSlides.Length; i++)
        {
            XRSimpleInteractable interactable = tutorialSlides[i].GetComponentInChildren<XRSimpleInteractable>();
            if (interactable != null)
            {
                interactable.selectEntered.RemoveAllListeners();
                int currentIndex = i;

                if (i < tutorialSlides.Length - 1)
                {
                    interactable.selectEntered.AddListener((args) => {
                        GoToNextSlide(currentIndex);
                    });
                }
                else
                {           
                    interactable.selectEntered.AddListener((args) => {
                        CloseTutorial();
                    });
                }
            }
        }
    }

    public void GoToNextSlide(int currentIndex)
    {
        tutorialSlides[currentIndex].SetActive(false);
        int nextIndex = currentIndex +  1;
        if (nextIndex < tutorialSlides.Length)
        {
            tutorialSlides[nextIndex].SetActive(true);
            currentSlideIndex = nextIndex;
        }
    }

    public void CloseTutorial()
    {
         tutorialCanvas.SetActive(false);
          Time.timeScale = 1f;
    }
}