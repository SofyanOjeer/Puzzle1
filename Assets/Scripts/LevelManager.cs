using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using SimpleJSON;
using System.Threading.Tasks;
using System;

public class LevelManager {
    private static LevelManager instance = null;

    private readonly string path = null;

    public enum Difficult { 
        A, B, C, D, E, F, G, H, J, T
       
    }
    private Dictionary<Difficult, List<Level>> levels = new Dictionary<Difficult, List<Level>>();
    
    public static LevelManager Instance {
        get {
            if (instance == null) instance = new LevelManager();
            return instance;
        }
    }

    public Difficult SelectedDifficult { get; set; } = Difficult.A;
    public Level SelectedLevel { get; private set; } = null;
    public List<Level> DifficultLevels { get { return levels[SelectedDifficult]; } }

    private LevelManager() {
        path = Path.Combine(Application.persistentDataPath, "Levels");

        if (Directory.Exists(path)) {
            Debug.LogWarning("Delete Levels directory from persistentDataPath");
            Directory.Delete(path, true);
        }

        DeserializeLevels();
    }

    public void StartLevel(int index) {
        Debug.Log($"Start {SelectedDifficult} level: {index}");
        SelectedLevel = DifficultLevels[index];
        LoadGameScene();
    }

    public void AddMatchHistory(bool won, int moves, int elapsedSeconds) {
        DateTime date = DateTime.Now;
        SelectedLevel.AddNewMatch(won, moves, date, elapsedSeconds);
        SelectedLevel.SaveToFile();
    }

    private async void LoadGameScene() {
        Debug.Log("Load game scene");

        var asyncLoad = SceneManager.LoadSceneAsync("Game");
        while (!asyncLoad.isDone) await Task.Delay(15);
    }

    private void DeserializeLevels() {
        if(Directory.Exists(path)) {
            Debug.Log("Start deserializing levels");

            levels[Difficult.A] = DeserializeDifficult("A");
            levels[Difficult.B] = DeserializeDifficult("B");
            levels[Difficult.C] = DeserializeDifficult("C");
            levels[Difficult.D] = DeserializeDifficult("D");
            levels[Difficult.E] = DeserializeDifficult("E");
            levels[Difficult.F] = DeserializeDifficult("F");
            levels[Difficult.G] = DeserializeDifficult("G");
            levels[Difficult.H] = DeserializeDifficult("H");

            levels[Difficult.J] = DeserializeDifficult("J");

            levels[Difficult.T] = DeserializeDifficult("T");


            string debugMessage = "Deserialization finished, found levels:";
            foreach (var diff in levels.Keys) debugMessage += $"\n\t{diff}: {levels[diff].Count}";
            Debug.Log(debugMessage);
        }
        else {
            Debug.Log("Levels directory not exists");
            CopyLevelsFromResources();
            DeserializeLevels();
        }
    }

    private List<Level> DeserializeDifficult(string directory) {
        var list = new List<Level>();
        var dirpath = Path.Combine(path, directory);
        var files = Directory.EnumerateFiles(dirpath, "*.json");
        Debug.Log($"Deserialize levels from {dirpath}");

        foreach(string filepath in files) {
            Debug.Log($"Load level from: {filepath}");

            var rawJson = File.ReadAllText(filepath);
            var node = JSON.Parse(rawJson);

            list.Add(new Level(filepath, node));
        }

        return list;
    }

    private void CopyLevelsFromResources() {
        Debug.Log("Copy levels from resources");

        Directory.CreateDirectory(path);
        CopyDifficultFromResources("A");
        CopyDifficultFromResources("B");
        CopyDifficultFromResources("C");
        CopyDifficultFromResources("D");
        CopyDifficultFromResources("E");
        CopyDifficultFromResources("F");
        CopyDifficultFromResources("G");
        CopyDifficultFromResources("H");

        CopyDifficultFromResources("J");

        CopyDifficultFromResources("T");
       

    }

    private void CopyDifficultFromResources(string name) {
        string levelPath = Path.Combine(path, name);
        Debug.Log($"Copy {name} difficult to: {levelPath}");
        Directory.CreateDirectory(levelPath);

        TextAsset[] files = Resources.LoadAll<TextAsset>($"Levels/{name}");
        Debug.Log($"Found files: {files.Length}");

        foreach(var file in files) {
            string filepath = Path.Combine(levelPath, file.name + ".json");
            Debug.Log($"Save {file.name}.json to {filepath}");

            File.WriteAllText(filepath, file.text);
        }
    }
}
