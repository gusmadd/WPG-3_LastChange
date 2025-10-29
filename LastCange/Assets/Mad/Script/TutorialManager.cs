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
        yield return ShowMessage("Don't ask how you ended up being... this.. whatever thing you are right now, or who am I. ");
        yield return ShowMessage("The story is quite long--or perhaps the developer team either dont have time to add the backstory or just... being lazy *cough*");
        yield return ShowMessage("anyway, Let's just cut to the chase shall we?");
        yield return new WaitUntil(() => PlayerMoved());
        yield return ShowMessage("You seems like already know how to walk around, or if not yet, try to use WASD on your keyboard");
        yield return ShowMessage("Good. ");
        yield return ShowMessage("Now if you think of a hell, what comes first in your mind?. ");
        yield return ShowMessage("Scary, boring, ugly, you name it. ");
        yield return ShowMessage("Is there a way out of here?. ");
        yield return ShowMessage("Perhaps yes, or not.. not really. ");

        // === 2. Lilin dan api ===
        yield return ShowMessage("There might be. Take a look at the Ritual Circle in the middle of the map. ");
        foreach (var lilin in lilins)
            lilin.SetActive(true);

        yield return ShowMessage("See those candles? you must lit them all to activate the Ritual Circle, you can find the firepit on either side of the map and bring the fire to lit them.");
        yield return ShowMessage("That is your Main Objective here. Don't ask why. ");
        fireSource.SetActive(true);
        yield return new WaitUntil(() => playerNearFire);
        yield return ShowMessage(" You can't stay on fire for too long though. It will hurt you. So be careful.");
        yield return ShowMessage("Press [E] to burn yourself.");
        fireTutorialShown = true;

        // === 3. Tunggu lilin menyala ===
        yield return new WaitUntil(() => AnyCandleLit());

        // === 4. Munculkan monster ===
        yield return ShowMessage("And, it's not really a hell if there's no those nasty looking demons.I.. meant it in the worse way. Unless if you're into them. Im not judging, eh.");
        if (playerCtrl) playerCtrl.enabled = false; // kunci player
        SpawnMonster();

        // tunggu monster menyentuh player
        yield return new WaitUntil(() => monsterTouchingPlayer);
        yield return ShowMessage("Try to get away from them. You can keep run and run away from them, but you can’t keep on running away forever, right?");

        // === 5. Kontak pertama dengan monster ===
        yield return HandleMonsterFirstContact();

        // === 6. Tunggu player berhasil dodge ===
        yield return new WaitUntil(() => PlayerFarFromMonster());

        // === 7. serangan balik
        yield return ShowMessage("Though, sometimes it's necessary to make your way through them using that sword attached to your limb [Press Space bar to attack].");
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
        yield return ShowMessage("Good, you've punish the monster.");

        // === 9. Akhiri tutorial (bisa lanjut ke scene berikut, misal) ===
        yield return ShowMessage("Now you can lit all the candles.");
        yield return new WaitUntil(() => AllCandlesLit());
        yield return ShowMessage("wow, looks something's happening..."); 
        yield return ShowMessage("enter the portal to escape this place.");
    }

    // ================== UTILITY ==================
    IEnumerator ShowMessage(string message)
    {
        // 🔒 Kunci player selama teks muncul
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

        // 🔓 Buka kunci player setelah teks ditutup
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
        // 🔒 Kunci player dan monster dulu
        if (playerCtrl) playerCtrl.enabled = false;
        if (currentMonsterScript) currentMonsterScript.StopMovement();

        // Tunggu sedikit biar textbox sempat muncul
        yield return new WaitForSeconds(0.25f);

        // 🔓 Izinkan player gerak untuk menghindar
        if (playerCtrl) playerCtrl.enabled = true;

        // 💡 Izinkan monster bergerak dan menyerang palsu (tanpa damage)
        if (currentMonsterScript)
        {
            currentMonsterScript.ResumeMovement();
            currentMonsterScript.canAttack = true;
            currentMonsterScript.noDamageInTutorial = true; // ⚠️ wajib tetap true!
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
