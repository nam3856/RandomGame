using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class TeamRandomizer : MonoBehaviour
{
    [Header("UI & 이펙트")]
    [SerializeField] private TMP_Text[] teamTexts;                  // 7개
    [SerializeField] private ParticleSystem[] teamEffects;          // 팀 완성 팡
    [SerializeField] private ParticleSystem[] teamMemberEffects;    // 멤버 등장 팡
    [SerializeField] private AudioSource effect;                    // 팡 사운드
    [SerializeField] private RandomMoveLight randomMoveLight;       // 라이트 포커스
    [SerializeField] private GameObject[] TeamTitles;          // 팀 이름들 (UI 오브젝트)

    [SerializeField] private GameObject SettingCanvas;          // 설정 UI
    [SerializeField] private Button shuffleButton;                         // 셔플 버튼
    [SerializeField] private GameObject CompletedText;                 // 완료 텍스트

    [Header("옵션")]
    [SerializeField] private Toggle noDuplicateToggle;              // 이전 팀 중복 방지 옵션
    [SerializeField] private Toggle fixedSeedToggle;                // 시드 고정 옵션
    [SerializeField] private TMP_InputField seedInputField;         // 시드 입력 필드

    [Header("플레이어 명단 (28명)")]
    [SerializeField] private List<string> players = new();          // 에디터에서 28명 입력

    [Header("Exit Button")]
    [SerializeField] private GameObject ExitButton;                // 종료 버튼

    private Dictionary<string, HashSet<string>> bannedPairs;
    private List<List<string>> teams = new();
    private List<string> teamsToShow = new();

    private System.Random rng;
    private void Awake()
    {
        // 토글 이벤트 등록
        fixedSeedToggle.onValueChanged.AddListener(OnFixedSeedToggleChanged);
        // 초깃값 반영
        OnFixedSeedToggleChanged(fixedSeedToggle.isOn);

        shuffleButton.onClick.AddListener(ShuffleTeams);
    }

    private void OnFixedSeedToggleChanged(bool isOn)
    {
        // 고정 시드 사용 설정에 따라 입력 필드 활성화
        seedInputField.gameObject.SetActive(isOn);
    }
    public void ShuffleTeams()
    {
        SettingCanvas.SetActive(false);

        // RNG 초기화: 시드 고정 옵션 확인
        if (fixedSeedToggle.isOn)
        {
            if (!int.TryParse(seedInputField.text, out int seed))
            {
                Debug.LogWarning("잘못된 시드값입니다. 기본 시드(0)로 고정합니다.");
                seed = 0;
            }
            rng = new System.Random(seed);
        }
        else
        {
            rng = new System.Random();
        }

        // 1) 파일에서 1회차 팀 불러오기
        var previousTeams = LoadPreviousTeamsFromFile();
        if (previousTeams == null || previousTeams.Count < 7)
        {
            Debug.LogError("PreviousTeams.txt를 읽어오지 못했거나, 7개 팀 정보가 아닙니다.");
            shuffleButton.interactable = true;
            return;
        }

        // 2) 이전 팀에서 함께 했던 쌍을 기록
        if (noDuplicateToggle.isOn)
            bannedPairs = BuildBannedPairs(previousTeams);
        else
            bannedPairs = new Dictionary<string, HashSet<string>>();

        // 3) 즉시 2회차 팀 생성
        teams = GenerateRound2Teams(players, bannedPairs, maxAttempts: 10000);
        if (teams == null)
        {
            Debug.LogWarning("유효한 2회차 팀 구성을 찾지 못했습니다.");
            shuffleButton.interactable = true;
            return;
        }

        SaveResultsToCsv();

        // 4) 연출 시작
        StartCoroutine(PlayTeamReveal());
    }

    // --------------------------------------------------------
    // PreviousTeams.txt 로드
    private List<List<string>> LoadPreviousTeamsFromFile()
    {
        // Application.dataPath는 빌드 시 *Data 폴더* 경로이므로, 상위 폴더(exe가 있는 위치)로 이동
        string dataPath = Application.dataPath;
        string exeFolder = Path.GetDirectoryName(dataPath);
        string filePath = Path.Combine(exeFolder, "PreviousTeams.txt");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"파일을 찾을 수 없습니다: {filePath}");
            return null;
        }

        var lines = File.ReadAllLines(filePath);
        var teams = new List<List<string>>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("팀"))
            {
                if (i + 1 < lines.Length)
                {
                    var members = lines[i + 1]
                        .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    if (members.Count == 4)
                    {
                        teams.Add(members);
                        teamsToShow.AddRange(members);
                        if(players.Count < 28)
                            players.AddRange(members);
                    }
                    else
                        Debug.LogWarning($"라인 형식 오류 (멤버 수 !=4): {lines[i + 1]}");
                }
            }
        }

        return teams;
    }
    // --------------------------------------------------------
    // CSV 파일로 결과 저장
    private void SaveResultsToCsv()
    {
        string dataPath = Application.dataPath;
        string exeFolder = Path.GetDirectoryName(dataPath);
        string filePath = Path.Combine(exeFolder, "Result.csv");

        try
        {
            using (var sw = new StreamWriter(filePath))
            {
                sw.WriteLine("Team,Member1,Member2,Member3,Member4");
                for (int i = 0; i < teams.Count; i++)
                {
                    var members = teams[i];
                    sw.WriteLine($"Team{i + 1},{members[0]},{members[1]},{members[2]},{members[3]}");
                }
            }
            Debug.Log($"Result.csv saved to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save Result.csv: {ex}");
        }
    }
    // --------------------------------------------------------
    // 팀 생성 로직
    private List<List<string>> GenerateRound2Teams(
        List<string> allPlayers,
        Dictionary<string, HashSet<string>> banned,
        int maxAttempts)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var shuffled = allPlayers.OrderBy(_ => rng.Next()).ToList();
            var candidate = new List<List<string>>();
            bool valid = true;

            for (int i = 0; i < 7; i++)
            {
                var group = shuffled.Skip(i * 4).Take(4).ToList();
                if (HasConflict(group, banned))
                {
                    valid = false;
                    break;
                }
                candidate.Add(group);
            }

            if (valid)
                return candidate;
        }
        return null;
    }


    private bool HasConflict(List<string> group, Dictionary<string, HashSet<string>> banned)
    {
        foreach (var a in group)
            foreach (var b in group)
                if (a != b && banned.TryGetValue(a, out var set) && set.Contains(b))
                    return true;
        return false;
    }

    private Dictionary<string, HashSet<string>> BuildBannedPairs(List<List<string>> teams)
    {
        var dict = new Dictionary<string, HashSet<string>>();
        foreach (var team in teams)
        {
            foreach (var p in team)
                if (!dict.ContainsKey(p))
                    dict[p] = new HashSet<string>();

            for (int i = 0; i < team.Count; i++)
                for (int j = i + 1; j < team.Count; j++)
                {
                    dict[team[i]].Add(team[j]);
                    dict[team[j]].Add(team[i]);
                }
        }
        return dict;
    }

    // --------------------------------------------------------
    // 팀 공개 연출
    private IEnumerator PlayTeamReveal()
    {
        yield return new WaitForSeconds(30f);
        for (int ti = 0; ti < teams.Count; ti++)
        {
            var group = teams[ti];
            teamTexts[ti].text = "";
            TeamTitles[ti].SetActive(true);

            foreach (var name in group)
            {
                // 포커스 & 라이트
                CameraFocusController.Instance.FocusOnTeam(teamTexts[ti].transform, 2.8f, 0.4f);
                randomMoveLight.FocusLightOnTeam(ti);
                teamMemberEffects[ti].gameObject.SetActive(true);
                teamMemberEffects[ti].Play();
                yield return StartCoroutine(PlayNameRoulette(ti, name));
                // 멤버 등장 이펙트
                
                effect.Play();

                // 즉시 이름 출력
                teamTexts[ti].text += (teamTexts[ti].text == "" ? "" : "\n");
                teamsToShow.Remove(name);

                // 텍스트 팝 애니메이션
                teamTexts[ti].transform
                    .DOScale(1.1f, 0.08f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => teamTexts[ti].transform.DOScale(1f, 0.08f));

                teamMemberEffects[ti].gameObject.transform.position = new Vector3(teamMemberEffects[ti].gameObject.transform.position.x, teamMemberEffects[ti].gameObject.transform.position.y-0.45f);
                yield return new WaitForSeconds(0.3f);
            }

            // 팀 완성 팡!
            teamEffects[ti].gameObject.SetActive(true);
            teamEffects[ti].Play();
            SoundManager.Instance.Play("Pop");
            teamTexts[ti].transform
                .DOShakeScale(0.6f, 0.6f, 10, 90);

            yield return new WaitForSeconds(0.5f);
            CameraFocusController.Instance.ResetFocus(0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        randomMoveLight.GoLight();

        yield return new WaitForSeconds(0.5f);
        CompletedText.SetActive(true);
        ExitButton.SetActive(true);
    }

    private IEnumerator PlayNameRoulette(int teamIndex, string finalName)
    {
        float duration = UnityEngine.Random.value +0.8f;
        float elapsed = 0f;
        float interval = 0.05f;

        var candidates = teamsToShow.ToList();

        candidates.Add("<color=#FFD700><size=120%>김홍일 강사님</size></color>");
        candidates.Add("<color=#FFD700><size=120%>최태온 강사님</size></color>");

        if (!candidates.Contains(finalName))
            candidates.Add(finalName); // 혹시 빠졌을 경우 대비

        while (elapsed < duration)
        {
            string randomName = PickWeightedName(candidates);
            teamTexts[teamIndex].text = ReplaceLastLine(teamTexts[teamIndex].text, randomName);

            teamMemberEffects[teamIndex].Play();
            effect.Play();

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // 최종 이름으로 고정
        teamTexts[teamIndex].text = AppendFinalName(teamTexts[teamIndex].text, finalName);
    }

    private string ReplaceLastLine(string original, string newLine)
    {
        var lines = original.Split('\n').ToList();
        if (lines.Count > 0)
            lines[lines.Count - 1] = newLine;
        return string.Join("\n", lines);
    }

    private string AppendFinalName(string currentText, string finalName)
    {
        var lines = currentText.Split('\n').ToList();
        if (lines.Contains(finalName)) return currentText; // 이미 있음
        lines[lines.Count - 1] = finalName;
        return string.Join("\n", lines);
    }

    string PickWeightedName(List<string> candidates)
    {
        string picked;
        while (true)
        {
            picked = candidates[UnityEngine.Random.Range(0, candidates.Count)];

            if (picked.Contains("강사님"))
            {
                if (UnityEngine.Random.value < 0.05f)
                    return picked;
            }
            else
            {
                return picked;
            }
        }
    }
}
