using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class TeamRandomizer : MonoBehaviour
{
    [SerializeField] private TMP_Text[] teamTexts; // 팀별 텍스트 (7개)
    [SerializeField] private ParticleSystem[] teamEffects; // 팡 이펙트 (7개)
    [SerializeField] private ParticleSystem[] teamMemberEffects; // 팡 이펙트 (7개)
    [SerializeField] private ParticleSystem[] teamMemberEffectsComplete; // 팡 이펙트 (7개)
    [SerializeField] private AudioSource effect;
    [SerializeField] private Button button;

    [SerializeField] private List<string> players = new();
    private List<List<string>> teams = new();

    [SerializeField] private RandomMoveLight randomMoveLight;

    Dictionary<string, HashSet<string>> previousTeammates = new();
    
    public void ShuffleTeams()
    {
        button.interactable = false;
        StopAllCoroutines();
        StartCoroutine(ShuffleAndAssign());
    }

    private IEnumerator ShuffleAndAssign()
    {
        var shuffled = players.OrderBy(x => Random.value).ToList();
        List<List<string>> previousTeams = new()
        {
            new() { "홍명진", "남경민", "박정연", "박수현" },
            new() { "박미르", "박순홍", "박강", "김경호" },
            new() { "이준엽", "박진혁", "양성일", "손정휘" },
            new() { "전태준", "차수민", "김형진", "이여진" },
            new() { "서민주", "이형근", "나기연", "허정범" },
            new() { "김나현", "이상진", "심형준", "박우영" },
            // ...
        };

        previousTeammates = new Dictionary<string, HashSet<string>>();

        foreach (var team in previousTeams)
        {
            foreach (var member in team)
            {
                if (!previousTeammates.ContainsKey(member))
                    previousTeammates[member] = new HashSet<string>();

                foreach (var teammate in team)
                {
                    if (teammate != member)
                        previousTeammates[member].Add(teammate);
                }
            }
        }
        // 팀 초기화
        teams = new();
        for (int i = 0; i < 7; i++) teams.Add(new List<string>());

        yield return new WaitForSeconds(30f); // 잠시 대기
        var assigned = new HashSet<string>();
        for (int teamIndex = 0; teamIndex < 7; teamIndex++)
        {
            teamTexts[teamIndex].text = ""; // 초기화
            for (int i = 0; i < 4; i++)
            {
                CameraFocusController.Instance.FocusOnTeam(teamTexts[teamIndex].transform, 2.8f, 0.4f);
                randomMoveLight.FocusLightOnTeam(teamIndex);

                string finalName = shuffled[teamIndex * 4 + i];
                
                var unassigned = players.Where(p => !assigned.Contains(p)).ToList();

                teamMemberEffects[teamIndex].gameObject.SetActive(true);
                teamMemberEffects[teamIndex].Play();
                int randomIndex = Random.Range(8, 40);

                int j = 0;
                while (j < randomIndex)
                {
                    string randomName = unassigned[Random.Range(0, unassigned.Count)];

                    if (IsDuplicateFromPreviousTeam(randomName, teams[teamIndex]))
                    {
                        continue;
                    }
                    effect.Play();

                    teamTexts[teamIndex].text = teams[teamIndex].Count == 0
                        ? $"{randomName}"
                        : string.Join("\n", teams[teamIndex]) + $"\n{randomName}";
                    yield return new WaitForSeconds(0.005f * j);

                    j++; // 조건 통과했을 때만 증가
                }

                assigned.Add(finalName); // 💡 배정된 인원 등록
                teams[teamIndex].Add(finalName);
                teamTexts[teamIndex].text = string.Join("\n", teams[teamIndex]);

                teamTexts[teamIndex].transform
                    .DOScale(1.1f, 0.08f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => teamTexts[teamIndex].transform.DOScale(1f, 0.08f));

                // ✅ 팀 완성되었을 때만 팡!
                if (teams[teamIndex].Count == 4)
                {
                    // 강한 팡 효과
                    teamEffects[teamIndex].gameObject.SetActive(true);
                    teamEffects[teamIndex].Play();
                    SoundManager.Instance.Play("Pop");

                    // 흔들기
                    teamTexts[teamIndex].transform
                        .DOShakeScale(0.6f, 0.6f, 10, 90);

                    yield return new WaitForSeconds(1.5f);

                    CameraFocusController.Instance.ResetFocus(0.5f);

                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    SoundManager.Instance.Play("");
                    teamMemberEffectsComplete[teamIndex].gameObject.SetActive(true);
                    teamMemberEffectsComplete[teamIndex].Play();
                    // 흔들기
                    teamTexts[teamIndex].transform
                        .DOShakeScale(0.3f, 0.3f, 10, 90);
                }

                yield return new WaitForSeconds(0.5f);
            }

        }
        randomMoveLight.MoveLightAgain();
        button.gameObject.SetActive(false);
    }

    bool IsDuplicateFromPreviousTeam(string candidate, List<string> currentTeam)
    {
        if (!previousTeammates.ContainsKey(candidate)) return false;

        foreach (var member in currentTeam)
        {
            if (previousTeammates[candidate].Contains(member))
                return true; // 과거에 한 팀이었음
        }

        return false;
    }
}
