using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

public class GameService : MonoBehaviour
{
    LoadManager _loadManager;
    #region Login 
    public void Initialize()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .RequestServerAuthCode(false).
            EnableSavedGames().
            Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        SignInUserWithPlaygames();
    }
    void SignInUserWithPlaygames()
    {
       GameServices.Instance.LogIn(LoginComplete);
    }

    private void LoginComplete (bool success)
    {
        if(success==true)
        {
            _loadManager.CallCoroutine();
        }
        else
        {

        }
    }
    
    public void Logout()
    {
        GameServices.Instance.LogOut();
    }

    #endregion

    #region Achievement
    private AchievementNames[] allAchievements;
    private LeaderboardNames[] allLeaderboards;
    private int indexNumberAchievements;
    public void CompleteAllAchievement()
    {
        GameServices.Instance.SubmitAchievement(allAchievements[indexNumberAchievements], SubmitComplete);
    }

    public void CompleteAchievement()
    {
        GameServices.Instance.SubmitAchievement(AchievementNames.Achievement);
    }

    public void ShowAchievementsUI()
    {
        GameServices.Instance.ShowAchievementsUI();
    }
    private void SubmitComplete (bool success, GameServicesError message)
    {
        if(success)
        {
        //achievement was submitted
        }
        else
        {
        //an error occurred
        Debug.LogError("Achievement failed to submit: " + message);
        }
    }

    private void CreateAchievementList()
    {
        int nrOfAchievements = System.Enum.GetValues(typeof(AchievementNames)).Length;
        allAchievements = new AchievementNames[nrOfAchievements];
        for (int i = 0; i < nrOfAchievements; i++)
        {
            allAchievements[i] = ((AchievementNames)i);
        }

    }
    #endregion

    #region Leaderboard
    private int _indexNumberLeaderboards;
    
    private void CreateLeaderboard()
    {
        int nrOfLeaderboards = System.Enum.GetValues(typeof(LeaderboardNames)).Length;
        allLeaderboards = new LeaderboardNames[nrOfLeaderboards];
        for (int i = 0; i < nrOfLeaderboards; i++)
        {
            allLeaderboards[i] = ((LeaderboardNames)i);
        }
    }

    public void ShowLeaderboadsUI()
    {
        GameServices.Instance.ShowLeaderboadsUI();
    }
    #endregion
    
    void Awake()
    {
        _loadManager = GetComponent<LoadManager>();
        CreateAchievementList();
        CreateLeaderboard();
    }
}
