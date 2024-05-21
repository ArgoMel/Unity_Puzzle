using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    /*
    public int data
    {
        get;
        private set;
    }
    */
    public enum SFX
    {
        LevelUp,
        Next,
        Attach,
        Button,
        GameOver
    }
    Dongle recentDongle;
    bool isOver;
    int poolIndex;
    int score;
    int sfxCursor;
    public int GetScore()
    {
        return score;
    }
    public void SetScore(int value)
    {
        score = value;
    }
    [Header("[ Core ]")]
    public int maxLevel;
    [Header("[ Object Pooling ]")]
    [Range(1, 30)] [SerializeField] int poolSize;
    public GameObject donglePref;
    public GameObject effectPref;
    public Transform dongleGroup;
    public Transform effectGroup;
    public List<Dongle> donglePool;
    public List<ParticleSystem> effectPool;
    [Header("[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    [Header("[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI maxScoreText;
    public TextMeshProUGUI subScoreText;
    [Header("[ ETC ]")]
    public GameObject line;
    public GameObject bottom;
    private void Awake()
    {
        Application.targetFrameRate = 60; //프레임 고정
        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int i = 0; i < poolSize; ++i)
        {
            MakeDongle();
        }
        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        else
        {
            maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
        }
    }
    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) 
        {
            Application.Quit();
        }
    }
    private void LateUpdate()
    {
        scoreText.text =score.ToString();
    }
    public void GameStart() 
    {
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        bgmPlayer.Play();
        SFXPlay(SFX.Button);
        StartCoroutine(CoolTime());
        NextDongle();
    }
    IEnumerator CoolTime() 
    {
        yield return new WaitForSeconds(1.5f);
    }
    Dongle MakeDongle() 
    {
        GameObject initEffectObj = Instantiate(effectPref, effectGroup);
        initEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem initEffect = initEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(initEffect);
        GameObject init = Instantiate(donglePref, dongleGroup);
        init.name = "Dongle" + donglePool.Count;
        Dongle initDongle = init.GetComponent<Dongle>();
        initDongle.SetGM(this);
        initDongle.SetEffect(initEffect);
        donglePool.Add(initDongle);
        return initDongle;
    }
    Dongle GetDongle()
    {
        for(int i = 0; i < donglePool.Count; ++i) 
        {
            poolIndex=(poolIndex+1)%donglePool.Count;
            if (!donglePool[poolIndex].gameObject.activeSelf)
            {
                return donglePool[poolIndex];
            }
        }
        return MakeDongle();
    }
    void NextDongle() 
    {
        if (isOver) 
        {
            return;
        }
        recentDongle = GetDongle();
        recentDongle.level =Random.Range(0, maxLevel);
        recentDongle.gameObject.SetActive(true);
        SFXPlay(SFX.LevelUp);
        StartCoroutine(WaitNext());
    }
    IEnumerator WaitNext() 
    {
        while (recentDongle) 
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);
        NextDongle();
    }
    public void TouchDown() 
    {
        if (!recentDongle) 
        {
            return;
        }
        recentDongle.Drag();
    }
    public void TouchUp() 
    {
        if (!recentDongle)
        {
            return;
        }
        recentDongle.Drop();
        recentDongle = null;
    }
    public void GameOver() 
    {
        if (isOver) 
        {
            return;
        }
        isOver = true;
        StartCoroutine(GameOverRoutine());
    }
    IEnumerator GameOverRoutine() 
    {
        Dongle[] dongles = FindObjectsOfType<Dongle>();
        for (int i = 0; i < dongles.Length; ++i)
        {
            dongles[i].GetRigid().simulated = false;
        }
        for (int i = 0; i < dongles.Length; ++i)
        {
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);
        int maxScore =Mathf.Max(score,PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        maxScoreText.text = maxScore.ToString();
        subScoreText.text = "Score : "+scoreText.text;
        endGroup.SetActive(true);
        bgmPlayer.Stop();
        SFXPlay(SFX.GameOver);
    }
    public void Reset()
    {
        SFXPlay(SFX.Button);
        StartCoroutine(ResetRoutine());
    }
    IEnumerator ResetRoutine() 
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }
    public void SFXPlay(SFX type) 
    {
        switch (type) 
        {
            case SFX.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0,3)];
                break;
            case SFX.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case SFX.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case SFX.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case SFX.GameOver:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor=(sfxCursor+1)%sfxPlayer.Length;
    }
}
