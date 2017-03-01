using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;


public class GameSceneManager : MonoBehaviour {

    // General
    private GameObject StartCanvas;
    private SpawnZombie ZombieGenerator;

    //Music Control
    private AudioSource IntroMusic;
    private AudioSource GameMusic;
    private bool MusicEnabled = true;

    //Difficulty
    private Slider DifficultySlider;
    private NavMeshAgent HumanoidNavMesh;

    // Gravity
    public Rigidbody[] LightBallBodies;
    private bool GravityEnabled = true;

    public Transform[] LightBallTransforms; 

    private void Start()
    {
        StartCanvas = GameObject.Find("StartCanvas");
        ZombieGenerator = GameObject.Find("ZombieGenerator").GetComponent<SpawnZombie>();
        IntroMusic = GameObject.Find("IntroMusic").GetComponent<AudioSource>();
        GameMusic = GameObject.Find("GameMusic").GetComponent<AudioSource>();

        DifficultySlider = GameObject.Find("DifficultySlider").GetComponent<Slider>();
        HumanoidNavMesh = GameObject.Find("Humanoid").GetComponent<NavMeshAgent>();

    }


    //Scene Switch Management
    public void ChangeToTENSScene()
    {
        SceneManager.LoadScene("TENSStudio", LoadSceneMode.Single);
    }

    public void ChangeToRockawayMainScene()
    {
        SceneManager.LoadScene("SpaceShipScene", LoadSceneMode.Single);
    }

    // Game Options
    public void ToggleGameSound()
    {
        MusicEnabled = !MusicEnabled;

        if (MusicEnabled)
        {
            IntroMusic.Play();
        } else
        {
            IntroMusic.Pause();
        }
    }

    public void ToggleGameGravity()
    {
        GravityEnabled = !GravityEnabled;
        for (int i=0; i<LightBallBodies.Length; i++)
        {
            LightBallBodies[i].useGravity = GravityEnabled;
        }

    }

    public void AdjustGameDifficulty()
    {
        float value = DifficultySlider.value;
        HumanoidNavMesh.speed = 3.0F * DifficultySlider.value + 1F;
    }

    //Game Start
    public void StartGame()
    {
        DismissStartPanel();
        ZombieGenerator.StartGenerateZombies();
        IntroMusic.Stop();

        if(MusicEnabled)
        {
            GameMusic.Play();
        }

        //Hack
        if (!GravityEnabled)
        {
            for(int i=0; i<LightBallTransforms.Length; i++)
            {
                LightBallTransforms[i].position = new Vector3(99, 99, 99);
            }
        }
        
    }

    private void DismissStartPanel()
    {
        StartCanvas.SetActive(false);
    }
}
