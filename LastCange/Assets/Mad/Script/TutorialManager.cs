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

    [Header("Tutorial Objects")]
    public GameObject[] lilins;
    public GameObject fireSource;
    public GameObject monsterPrefab;
    public Transform monsterSpawnPoint;

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
    private float moveCheckDelay = 1f; // waktu tunggu 1 detik
    private float moveTimer = 0f;
    private void Start()
    {
        playerCtrl = player.GetComponent<PlayerControler>();
        lastPosition = player.transform.position;
        StartCoroutine(TutorialSequence());
    }

    IEnumerator TutorialSequence()
    {
        // === 1. Intro ===
        yield return ShowMessage("Don't ask how you ended up being... this...<br>whatever thing you are right now...");
        yield return ShowMessage("<color=#bc282e><i>or who am I.</i></color>");
        yield return ShowMessage("The story is quite long--or perhaps the developer team<br>either dont have time to add the backstory or just... being lazy <i>*cough*</i>");
        yield return ShowMessage("anyway, Let's just cut to the chase shall we?");
        yield return new WaitUntil(() => PlayerMoved());
        yield return ShowMessage("You seems like already know how to <color=#bc282e>walk around</color>, or if not yet, try to use <color=#bc282e><b>W A S D</b></color> on your keyboard");
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
        if (playerCtrl) playerCtrl.enabled = false; // kunci player
        SpawnMonster();

        // tunggu monster menyentuh player
        yield return new WaitUntil(() => monsterTouchingPlayer);
        yield return ShowMessage("Try to get away from them. You can keep run and run away from them.");

        // === 5. Kontak pertama dengan monster ===
        yield return HandleMonsterFirstContact();

        // === 6. Tunggu player berhasil dodge ===
        yield return new WaitUntil(() => PlayerFarFromMonster());

        // === 7. serangan balik
        yield return ShowMessage("But you can’t keep on running away forever, right?");
        yield return ShowMessage("Though, sometimes it's necessary to make your way through them using that <color=#bc282e>sword</color> attached to your limb<br><color=#6cc74b><i>[Press Space bar to attack].</i></color>");
        yield return new WaitForSeconds(0.3f);
        // aktifkan player & monster untuk pertempuran
        if (playerCtrl)
        {
            playerCtrl.enabled = true;
            playerCtrl.UnlockMovement();
            playerCtrl.canAttack = true; // pastikan kamu punya variabel ini di PlayerControler
        }

        if (currentMonsterScript)
        {
            currentMonsterScript.StopAllCoroutines(); // hentikan semua serangan lama
            currentMonsterScript.noDamageInTutorial = false;
            currentMonsterScript.canAttack = true;
            currentMonsterScript.ResumeMovement();
        }


        // === 8. Tunggu sampai monster mati ===
        yield return new WaitUntil(() => currentMonster == null || currentMonsterScript == null);
        yield return ShowMessage("Good job there.");

        // === 9. Akhiri tutorial (bisa lanjut ke scene berikut, misal) ===
        yield return ShowMessage("Now try lit <color=#bc282e>all the candles.</color>");
        yield return new WaitUntil(() => AllCandlesLit());
        yield return ShowMessage("Go inside the portal to escape this place, <i>or so you wish.</i>");
    }

    // ================== UTILITY ==================
    IEnumerator ShowMessage(string message)
    {
        // ?? Kunci player selama teks muncul
        if (playerCtrl != null)
            playerCtrl.LockMovement();

        textBoxPanel.SetActive(true);
        tutorialText.text = "";

        foreach (char c in message)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // tunggu sampai pemain tekan Enter
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        // ?? Buka kunci player setelah teks ditutup
        textBoxPanel.SetActive(false);
        if (playerCtrl != null)
            playerCtrl.UnlockMovement();
    }

    private bool PlayerMoved()
{
    // tunggu dulu 1 detik sebelum mulai deteksi pergerakan
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
            currentMonsterScript.canAttack = false; // monster belum boleh nyerang
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
        // ?? Kunci player dan monster dulu
        if (playerCtrl) playerCtrl.enabled = false;
        if (currentMonsterScript) currentMonsterScript.StopMovement();

        // Tunggu sedikit biar textbox sempat muncul
        yield return new WaitForSeconds(0.25f);

        // ?? Izinkan player gerak untuk menghindar
        if (playerCtrl) playerCtrl.enabled = true;

        // ?? Izinkan monster bergerak dan menyerang palsu (tanpa damage)
        if (currentMonsterScript)
        {
            currentMonsterScript.ResumeMovement();
            currentMonsterScript.canAttack = true;
            currentMonsterScript.noDamageInTutorial = true; // ?? wajib tetap true!
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

}
