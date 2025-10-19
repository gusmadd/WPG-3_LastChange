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

    private void Start()
    {
        playerCtrl = player.GetComponent<PlayerControler>();
        lastPosition = player.transform.position;
        StartCoroutine(TutorialSequence());
    }

    IEnumerator TutorialSequence()
    {
        // === 1. Intro ===
        yield return ShowMessage("Sugeng Rawuh dumeteng panjenengan ten mriki, Kadunyan neraaka kang katah genine.");
        yield return ShowMessage("Panjenengan saget ngagem tombol W, A, S, D, kagem nggerakaken awakmu.");
        yield return new WaitUntil(() => PlayerMoved());

        // === 2. Lilin dan api ===
        yield return ShowMessage("Di tempat ritual ini terdapat beberapa lilin, dan kamu harus menyalakan lilin tersebut.");
        foreach (var lilin in lilins)
            lilin.SetActive(true);

        yield return ShowMessage("Di sekeliling tempat ini ada sumber api. Kamu harus mencarinya dan membakar dirimu untuk menyalakan lilin tersebut.");
        fireSource.SetActive(true);
        yield return new WaitUntil(() => playerNearFire);
        yield return ShowMessage("Ketika kamu terbakar, akan muncul pain tolerance. Itu adalah waktumu ketika terbakar, jadi jangan kelamaan.");
        yield return ShowMessage("Tekan [E] untuk membakar diri.");
        fireTutorialShown = true;

        // === 3. Tunggu lilin menyala ===
        yield return new WaitUntil(() => AnyCandleLit());

        // === 4. Munculkan monster ===
        yield return ShowMessage("Ada sesuatu muncul...");
        if (playerCtrl) playerCtrl.enabled = false; // kunci player
        SpawnMonster();

        // tunggu monster menyentuh player
        yield return new WaitUntil(() => monsterTouchingPlayer);
        yield return ShowMessage("Kamu harus bisa menghindari serangan dari monster tersebut.");

        // === 5. Kontak pertama dengan monster ===
        yield return HandleMonsterFirstContact();

        // === 6. Tunggu player berhasil dodge ===
        yield return new WaitUntil(() => PlayerFarFromMonster());
        yield return ShowMessage("Bagus! Kamu berhasil menghindar dari serangan monster.");

        // === 7. serangan balik
        yield return ShowMessage("Sekarang kamu bisa menyerang monster tersebut sampai ia mati. gunakan tombo; spasi untuk menyerang");
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
        yield return ShowMessage("Bagus! Kamu berhasil mengalahkan monster tersebut.");

        // === 9. Akhiri tutorial (bisa lanjut ke scene berikut, misal) ===
        yield return ShowMessage("Kamu bisa menyalakan semua lilinya");
        yield return new WaitUntil(() => AllCandlesLit());
        yield return ShowMessage("✨ Semua lilin telah menyala! Portal telah terbuka."); yield return ShowMessage("Masuklah ke portal untuk lanjut ke level selanjutnya.");
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
