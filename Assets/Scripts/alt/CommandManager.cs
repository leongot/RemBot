using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CommandType
{
    CT_AVANTI,
    CT_SINISTRA,
    CT_DESTRA,
    CT_INDIETRO,
    CT_SPARA,
    CT_IF_ELSE,
    CT_WHILE,
    CT_LIBERO,
    CT_MURO, 
    CT_NEMICO
}

public class CommandManager : MonoBehaviour
{
    [SerializeField]
    GameObject CommandAvanti,
               CommandSinistra,
               CommandDestra,
               CommandIndietro,
               CommandSpara,
               CommandWhile,
               CommandIfElse,
               CommandLibero,
               CommandMuro,
               CommandNemico;

    [SerializeField]
    Transform programArea;

    Transform activeCommmand,
              activeCondition;


    CommandType[] buffer = new CommandType[1024]; // Very big buffer

    Dictionary<CommandType, GameObject> lut;

    void Awake()
    {
        // Genera la look up table dei comandi a runtime
        lut = new()
        {
            { CommandType.CT_AVANTI,   CommandAvanti },
            { CommandType.CT_SINISTRA, CommandSinistra },
            { CommandType.CT_DESTRA,   CommandDestra },
            { CommandType.CT_INDIETRO, CommandIndietro },
            { CommandType.CT_SPARA,    CommandSpara },
            { CommandType.CT_IF_ELSE,  CommandIfElse },
            { CommandType.CT_WHILE,    CommandWhile },
            { CommandType.CT_LIBERO,   CommandLibero },
            { CommandType.CT_MURO,     CommandMuro },
            { CommandType.CT_NEMICO,   CommandNemico },
        };

        activeCommmand = programArea;
    }

    public void ParseCommand()
    {
        
    }

    public void GetPrefab(CommandType type)
    {
        GameObject command = lut[type];
    }
}
