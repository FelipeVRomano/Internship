using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;

public class SaveManager : MonoBehaviour
{
    //Script responsible for Local and Cloud save
    #region Important Values

    int _localCoinsValue;
    int _cloudCoinsValue;
    int _baseCoinsValue;

    string _localTextValue;
    string _cloudTextValue;
    string _baseTextValue;
    
    int _baseScoreValue;

    bool _saveCloud;
    bool _saveLocal;
    
    #endregion
    
    string _fullPath;
    SavedValues _savedValues = new SavedValues();
    bool _encrypt = false;
    string _textUICloudSave;
    bool _logged;
    
    //cloud save
    private bool _isSaving = false;
    private string SAVE_NAME = "savegames";
    bool _executingCoroutine;
    LoadManager _loadManager;
    
    //Debug text
    public InputField TextUI;
    public Text TextCoins;
    public Text TextUISave;
    
    public OnLoginEvent[] _OnLoginEvent;
    
    void Awake()
    {
        _fullPath = Application.persistentDataPath + "/" + "LocalFileName";
        _loadManager = GetComponent<LoadManager>();
    }
    
    public void ActivateGameObjects()
    {
         for(int i = 0; i < _OnLoginEvent.Length; i++)
            _OnLoginEvent[i].screenObj.SetActive(_OnLoginEvent[i].activateGameObject);
    }
    
    public void LoadGameFunction()
    {
        StartCoroutine(LoadGame());
    }
    IEnumerator LoadGame()
    {
        if(!_executingCoroutine)
        {
            _executingCoroutine = true;
            _saveLocal = false;
            _saveCloud = false;
            LoadGameValues();
            OpenSavetoCloud(false);
            while(!_saveLocal || !_saveCloud)
            yield return null;
            
            if(_cloudCoinsValue != _localCoinsValue)
            {
                _baseCoinsValue = _cloudCoinsValue;
                _saveLocal = false;
            }
            else{
                _baseCoinsValue = _cloudCoinsValue;
            }

            if(_cloudTextValue != _localTextValue)
            {
                _baseTextValue = _localTextValue;
                _saveLocal = false;
            }
            else{
                _baseTextValue = _localTextValue;
            }

            if(_saveLocal != true)
                    SaveGameValues();
            while(!_saveLocal) yield return null;
            
            TextUISave.text += "Salvei final";
            LoadManager.LoadedValuesReady = true;
        }
    }
    
    //----------------------------------LOAD/SAVE-------------------------------------------------
    public void SaveGame()
    {
        OpenSavetoCloud(true);
        SaveGameValues();
    }

    #region LOAD/SAVE LOCAL
    public void LoadGameValues()
    {
#if JSONSerializationFileSave || BinarySerializationFileSave
        logText = "\nLoad Started (File): " + fullPath;
#else
        logText = "\nLoad Started (PlayerPrefs): " + _fullPath;
#endif
        SaveManager.Instance.Load<SavedValues>(_fullPath, DataWasLoaded, _encrypt);
    }

     private void DataWasLoaded(SavedValues data, SaveResult result, string message)
    {
        logText = "\nData Was Loaded";
        logText = "\nresult: " + result + ", message: " + message;

        if (result == SaveResult.EmptyData || result == SaveResult.Error)
        {
            logText = "\nNo Data File Found -> Creating new data...";
            _saveLocal = true;
            _savedValues = new SavedValues();
        }

        if (result == SaveResult.Success)
        {
            _savedValues = data;
        }
        
        _localCoinsValue = _savedValues.Coins;
        AddScore(0);
        _localTextValue = _savedValues.Text;
        _saveLocal = true;
    }

    public void SaveGameValues()
    {
        logText = "\nSave Started";
        _savedValues.Coins = _baseCoinsValue;
        _savedValues.Text = _baseTextValue;

        GameServices.Instance.GetPlayerScore(LeaderboardNames.Score, ScoreLoaded);
        SaveManager.Instance.Save(_savedValues, _fullPath, DataWasSaved, _encrypt);
    }

    private void ScoreLoaded(long score)
    {
        _baseScoreValue = Convert.ToInt32(score);
        _savedValues.Score = _baseScoreValue;
    }
    
    private void SaveScore()
    {
        _savedValues.Score = _baseScoreValue;
        SaveManager.Instance.Save(_savedValues, _fullPath, DataWasSaved, _encrypt);
    }

    public void RefreshText()
    {
        _baseTextValue = TextUI.text;
    }
    
    private void DataWasSaved(SaveResult result, string message)
    {
        logText = "\nData Was Saved";
        logText = "\nresult: " + result + ", message: " + message;
        _saveLocal = true;
        if (result == SaveResult.Error)
        {
            logText = "\nError saving data";
        }
    }
    
    #endregion

    #region LOAD/SAVE CLOUD
    public void OpenSavetoCloud(bool saving)
    {

        if(Social.localUser.authenticated)
        {
            _isSaving = saving;
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution
                (SAVE_NAME, GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime, SavedGameOpen);     
        }
    }

    private void SavedGameOpen(SavedGameRequestStatus status, ISavedGameMetadata meta)
    {
        if(status == SavedGameRequestStatus.Success)
        {
            if (_isSaving)//if is saving is true we are saving our data to cloud
            {
                byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(GetDataToStoreinCloud());
                SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder().Build();
                ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(meta, update, data, SaveUpdate);
            }
            else//if is saving is false we are opening our saved data from cloud
            {
                 TextUISave.text += "Abrindo save";
                 ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(meta, ReadDataFromCloud);
            }
        }
    }

    void ReadDataFromCloud(SavedGameRequestStatus status, byte[] data)
    {
        if(status == SavedGameRequestStatus.Success)
        {
            string savedata = System.Text.ASCIIEncoding.ASCII.GetString(data);
            LoadDataFromCloudToOurGame(savedata);
        }
    }

    private void LoadDataFromCloudToOurGame(string savedata)
    {

        string[] data = savedata.Split('|');
        _saveCloud = true;
        TextUISave.text += "Salvei na cloud";
        _cloudTextValue = data[0].ToString();
        _cloudCoinsValue = int.Parse(data[1]);
    }

    private string GetDataToStoreinCloud()
    {
        string Data = "";
        Data += TextUI.text.ToString();
        Data += "|";
        Data += _baseCoinsValue.ToString();
        Data += "|";
        return Data;
    }
    #endregion

    #region EXTRA FUNCTIONS
    public void Clear()
    {
        logText += "\nClear";
        SaveManager.Instance.ClearFIle(_fullPath);
    }

    public void ClearLog()
    {
        logText = "";
    }

    public void AddCoins(int x)
    {
        _baseCoinsValue+=x;
        TextCoins.text = "Coins " + _baseCoinsValue.ToString();
    }

    public void RemoveCoins()
    {
        _baseCoinsValue--;
        TextCoins.text = "Coins " + _baseCoinsValue.ToString();
    }

    public void AddScore(int x)
    {
        _baseScoreValue += x;
        GameServices.Instance.SubmitScore(_baseScoreValue, LeaderboardNames.Score);
        SaveGame();
    }
    #endregion
}
