using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // pakai TextMeshPro

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject textBoxPanel;
    public TMP_Text tutorialText;
    public float typingSpeed = 0.03f;

    [Header("Typing Effect")]
    public AudioClip typeSound; // suara per huruf (opsional)

    [Header("Tutorial Objects")]
    public GameObject[] lilins;
    public GameObject fireSource;
    public GameObject monsterPrefab;
    public Transform monsterSpawnPoint;

    [Header("Portal")]
    public GameObject portalObject;

    [Header("Player Reference")]
    public GameObject player;
    private PlayerControler playerCtrl;

    // === STATE ===
    private bool playerNearFire = false;
    private bool fireTutorialShown = false;
    private bool monsterSpawned = false;
    private bool waitingMonsterContact = false;
    private bool monsterTouchingPlayer = false;

    private Vector3 lastPosition;
    private GameObject currentMonster;
    private EnemyIMO currentMonsterScript;
    private float moveCheckDelay = 1f;
    private float moveTimer = 0f;

    private void Start()
    {
        playerCtrl = player.GetComponent<PlayerControler>();
        lastPosition = player.transform.position;
        StartCoroutine(TutorialSequence());
    }

    IEnumerator TutorialSequence()
    {
        // 🌀 0. Tampilkan animasi spawn dulu
        if (playerCtrl != null)
        {
            yield return playerCtrl.PlaySpawnAnimation();
        }

        // === 1. Intro ===
        yield return ShowMessage("Don't ask how you ended up being... this...<br>whatever thing you are right now...");
        yield return ShowMessage("<color=#bc282e><i>or who am I.</i></color>");
        yield return ShowMessage("The story is quite long--or perhaps the developer team<br>either dont have time to add the backstory or just... being lazy <i>*cough*</i>");
        yield return ShowMessage("anyway, Let's just cut to the chase shall we?");
        yield return ShowMessage("You seems like already know how to <color=#bc282e>walk around</color>, or if not yet, try to use <color=#bc282e><b>W A S D</b></color> on your keyboard");

        playerCtrl.UnlockMovement();
        yield return new WaitUntil(() => PlayerMoved());
        playerCtrl.LockMovement();

        yield return ShowMessage("Good. ");
        yield return ShowMessage("Now if you think of a hell, <i>what comes first in your mind?.</i>");
        yield return ShowMessage("Scary, boring, ugly, you name it. ");
        yield return ShowMessage("Is there a <color=#bc282e>way out of here</color>?");
        yield return ShowMessage("Perhaps yes, or not.. not really. ");

        // === 2. Lilin dan api ===
        yield return ShowMessage("There might be. Take a look at the <color=#bc282e><b>Ritual Circle</b></color> in the middle of the map. ");
        foreach (var lilin in lilins)
            lilin.SetActive(true);

        yield return ShowMessage("See those <color=#bc282e><b>candles</b></color>? you must <color=#bc282e><b>lit</b></color> them all to <color=#6cc74b><i>activate</i></color> the <color=#bc282e><b>Ritual Circle.</b></color>");
        yield return ShowMessage("you can find the <color=#bc282e><b>firepit</b></color> on either side of the map and bring the fire to lit them.");
        yield return ShowMessage("That's your <color=#bc282e><i>mission</i></color> here. Don't ask why.");

        fireSource.SetActive(true);
        yield return new WaitUntil(() => playerNearFire);

        yield return ShowMessage("Don't stay on fire for too long though or you will get <color=#2b2b2b><b>burned</b></color> into ash.");
        yield return ShowMessage("You can <color=#6cc74b><i>press [E]</i></color> to burn yourself.");
        fireTutorialShown = true;

        // === 3. Tunggu lilin menyala ===
        yield return new WaitUntil(() => AnyCandleLit());

        // === 4. Munculkan monster ===
        yield return ShowMessage("And, it's not really a hell if there's no those nasty looking demons.<br>I.. meant it in the worse way.");
        yield return ShowMessage("<i>Unless if you're into them.</i> Im not judging, eh.");

        if (playerCtrl) playerCtrl.enabled = false;
        SpawnMonster();

        yield return new WaitUntil(() => IsEnemyTouchingPlayer());
        yield return ShowMessage("Try to get away from them. You can keep run and run away from them.");

        yield return HandleMonsterFirstContact();
        yield return new WaitUntil(() => PlayerFarFromMonster());

        yield return ShowMessage("But you can’t keep on running away forever, right?");
        yield return ShowMessage("Though, sometimes it's necessary to make your way through them using that <color=#bc282e>sword</color> attached to your limb<br><color=#6cc74b><i>[Press Space bar to attack].</i></color>");
        yield return new WaitForSeconds(0.3f);

        if (playerCtrl)
        {
            playerCtrl.enabled = true;
            playerCtrl.UnlockMovement();
            playerCtrl.canAttack = true;
        }

        if (currentMonsterScript)
        {
            currentMonsterScript.StopAllCoroutines();
            currentMonsterScript.noDamageInTutorial = false;
            currentMonsterScript.canAttack = true;
            currentMonsterScript.ResumeMovement();
        }

        yield return new WaitUntil(() => currentMonster == null || currentMonsterScript == null);
        yield return ShowMessage("Good job there.");

        yield return ShowMessage("Now try lit <color=#bc282e>all the candles.</color>");
        yield return new WaitUntil(() => AllCandlesLit());

        if (portalObject != null)
        {
            portalObject.SetActive(true);
            Debug.Log("✨ Portal diaktifkan oleh TutorialManager!");
        }
        else
        {
            Debug.LogWarning("⚠️ Portal belum dihubungkan ke TutorialManager!");
        }

        yield return ShowMessage("Go inside the portal to escape this place, <i>or so you wish.</i>");
    }

    // ================== SHOW MESSAGE (FIXED) ==================
    IEnumerator ShowMessage(string message)
    {
        if (playerCtrl != null)
            playerCtrl.LockMovement();

        textBoxPanel.SetActive(true);
        tutorialText.text = "";

        // Jalankan efek ngetik
        yield return StartCoroutine(TypeText(message));

        // Tunggu Enter dilepas dulu sebelum lanjut
        yield return new WaitUntil(() => !Input.GetKey(KeyCode.Return));

        // Tunggu Enter ditekan untuk lanjut ke kalimat berikut
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        // Tunggu Enter dilepas biar gak kebaca ke pesan berikut
        yield return new WaitUntil(() => !Input.GetKey(KeyCode.Return));

        textBoxPanel.SetActive(false);

        if (playerCtrl != null)
            playerCtrl.UnlockMovement();
    }

    // ================== TYPING EFFECT (FIXED) ==================
    IEnumerator TypeText(string message)
    {
        bool insideTag = false;
        bool skip = false;

        tutorialText.text = "";

        for (int i = 0; i < message.Length; i++)
        {
            char c = message[i];

            if (c == '<') insideTag = true;
            tutorialText.text += c;
            if (c == '>') insideTag = false;

            if (insideTag) continue;

            // kalau tekan Enter, tampilkan langsung semua teks
            if (Input.GetKeyDown(KeyCode.Return))
            {
                skip = true;
            }

            if (skip)
            {
                tutorialText.text = message;
                break;
            }

            // Delay per huruf
            if (c == '.' || c == '!' || c == '?')
                yield return new WaitForSeconds(typingSpeed * 10f);
            else if (c == ',' || c == ';' || c == ':')
                yield return new WaitForSeconds(typingSpeed * 5f);
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        // Pastikan Enter dilepas dulu sebelum lanjut
        yield return new WaitUntil(() => !Input.GetKey(KeyCode.Return));
    }

    // ================== UTILITIES ==================
    private bool PlayerMoved()
    {
        if (moveTimer < moveCheckDelay)
        {
            moveTimer += Time.deltaTime;
            return false;
        }

        float distanceMoved = Vector3.Distance(player.transform.position, lastPosition);
        lastPosition = player.transform.position;
        return distanceMoved > 0.05f;
    }

    private bool AnyCandleLit()
    {
        foreach (var lilin in lilins)
        {
            var candle = lilin.GetComponent<Candle>();
            if (candle != null && candle.isLit)
                return true;
        }
        return false;
    }

    private void SpawnMonster()
    {
        if (!monsterSpawned && monsterPrefab && monsterSpawnPoint)
        {
            currentMonster = Instantiate(monsterPrefab, monsterSpawnPoint.position, Quaternion.identity);
            currentMonsterScript = currentMonster.GetComponent<EnemyIMO>();
            currentMonsterScript.canAttack = false;
            currentMonsterScript.noDamageInTutorial = true;
            currentMonsterScript.SetTutorialManager(this);
            monsterSpawned = true;
        }
    }

    public void SetPlayerNearFire(bool value)
    {
        playerNearFire = value;
    }

    public void NotifyMonsterTouchedPlayer(EnemyIMO monster)
    {
        if (!monsterTouchingPlayer)
        {
            monsterTouchingPlayer = true;
            currentMonsterScript = monster;
        }
    }

    IEnumerator HandleMonsterFirstContact()
    {
        if (playerCtrl) playerCtrl.enabled = false;
        if (currentMonsterScript) currentMonsterScript.StopMovement();

        yield return new WaitForSeconds(0.25f);

        if (playerCtrl) playerCtrl.enabled = true;

        if (currentMonsterScript)
        {
            currentMonsterScript.ResumeMovement();
            currentMonsterScript.canAttack = true;
            currentMonsterScript.noDamageInTutorial = true;
        }
    }

    private bool PlayerFarFromMonster()
    {
        if (currentMonster == null || player == null) return false;
        float dist = Vector3.Distance(player.transform.position, currentMonster.transform.position);
        return dist > 3f;
    }

    private bool AllCandlesLit()
    {
        foreach (var lilin in lilins)
        {
            var candle = lilin.GetComponent<Candle>();
            if (candle != null && !candle.isLit)
                return false;
        }
        return true;
    }
    bool IsEnemyTouchingPlayer()
    {
        if (player == null) return false;

        // Adjust ukuran kotak deteksi sesuai ukuran player
        Vector2 size = new Vector2(1f, 1f);

        Collider2D hit = Physics2D.OverlapBox(
    player.transform.position,
    size,
    0f,
    LayerMask.GetMask("Monster")
);

        return hit != null;
    }
    private void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(player.transform.position, new Vector3(0.5f, 0.8f, 1));
    }
}
