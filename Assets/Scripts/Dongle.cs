using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;
//프리펩 리기드바디 들어가서 interpolate를 interpolate로 바꿔주면 부드러워진다
    Rigidbody2D rigid;
    public Rigidbody2D GetRigid()
    {
        return rigid;
    }
    CircleCollider2D circle;
    Animator anim;
    SpriteRenderer sr;
    GameManager gm;
    public GameManager GetGM()
    {
        return gm;
    }
    public void SetGM(GameManager manager) 
    {
        gm = manager;
    }
    ParticleSystem effect;
    public void SetEffect(ParticleSystem ps)
    {
        effect = ps;
    }
    float deadTime;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        anim.SetInteger("Level",level);
    }
    private void OnDisable()
    {
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine());
    }
    //projectsetting가서 autosync 켜줘야함
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag=="Dongle")
        {
            if(level > 6) 
            {
                return;
            }
            Dongle other = collision.gameObject.GetComponent<Dongle>();
            if (level == other.level&& !isMerge&&!other.isMerge) 
            {
                float x = transform.position.x;
                float y = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                if (y < otherY || (y == otherY && x > otherX)) 
                {
                    other.Hide(transform.position);
                    LevelUp();
                }
            }
        }
    }
    //rigidbody가서 sleeping mode를 never sleep으로 바꿔줘야한다
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag=="Finish") 
        {
            deadTime += Time.deltaTime;
            if (deadTime > 2) 
            {
                //sr.color = Color.red;
                sr.color = new Color(0.9f,0.2f,0.2f);
            }
            if (deadTime > 5) 
            {
                gm.GameOver();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag=="Finish") 
        {
            deadTime = 0;
            sr.color = Color.white;
        }
    }
    void Update()
    {
        if (isDrag) 
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBorder = -4.2f + transform.localScale.x * 0.5f;
            float rightBorder = 4.2f - transform.localScale.x * 0.5f;
            if (mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            else if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }
            mousePos.y = 8;
            mousePos.z = 0;
            //Lerp : 목표지점으로 부드럽게 이동(현재위치,목표위치,속도)
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
    }
    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }
    public void Hide(Vector3 pos) 
    {
        isMerge = true;
        rigid.simulated = false;
        circle.enabled = false;
        if (pos == Vector3.up * 100)
        {
            effect.Play();
        }
        //else 
        //{
        //    ParticleSystem[] pss = FindObjectsOfType<ParticleSystem>();
        //    for (int i = 0; i < pss.Length; ++i)
        //    {
        //        if (pss[i].transform.position == Vector3.zero) 
        //        {
        //            Destroy(pss[i]);
        //            Debug.Log("d");
        //        }
        //    }
        //}
        StartCoroutine(HideRoutine(pos));
    }
    IEnumerator HideRoutine(Vector3 pos) 
    {
        int frameCount = 0;
        while (frameCount < 20) 
        {
            ++frameCount;
            if (pos==Vector3.up*100)
            {
                transform.localScale = Vector3.Lerp(transform.localScale,Vector3.zero,0.2f);
            }
            transform.position = Vector3.Lerp(transform.position,pos,0.5f);
            yield return null;
        }
        gm.SetScore(gm.GetScore()+(int)Mathf.Pow(2, level)); //오른쪽 인자를 왼쪽인자만큼 거듭제곱
        isMerge = false;
        gameObject.SetActive(false);
    }
    void LevelUp() 
    {
        isMerge = true;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        StartCoroutine(LevelUpRoutine());
    }
    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        anim.SetInteger("Level",level+1);
        EffectPlay();
        gm.SFXPlay(GameManager.SFX.LevelUp);
        yield return new WaitForSeconds(0.3f);
        ++level;
        gm.maxLevel = Mathf.Max(level, gm.maxLevel);    //둘중에 큰거 선택
        isMerge = false;
    }
    void EffectPlay() 
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
    IEnumerator AttachRoutine()
    {
        if (isAttach) 
        {
            yield break;
        }
        isAttach = true;
        gm.SFXPlay(GameManager.SFX.Attach);
        yield return new WaitForSeconds(0.2f);
        isAttach = false;
    }
}
