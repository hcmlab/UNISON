using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableState : MonoBehaviour
{
    [SerializeField]
    public Stations.Zones.Interactable getPetitions;
    [SerializeField]
    public Stations.Zones.Interactable voteZone;
    [SerializeField]
    public Stations.Zones.Interactable petitions;
    [SerializeField]
    public Stations.Zones.Interactable earnMoney;
    [SerializeField]
    public Stations.Zones.Interactable learn;
    [SerializeField]
    public Stations.Zones.Interactable carePackage;
    [SerializeField]
    public Stations.Zones.Interactable help;
    [SerializeField]
    public Stations.Zones.Interactable relax;
    [SerializeField]
    public Stations.Zones.Interactable buy;
    [SerializeField]
    public Stations.Zones.Interactable invest;
    [SerializeField]
    public Stations.Zones.Interactable sendMoney;
    [SerializeField]
    public Stations.Zones.Interactable graphs;
    [SerializeField]
    public Stations.Zones.Interactable survey;
    [SerializeField]
    public Stations.Zones.Interactable globalInfo;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool InteractionInProgress()
    {
        return (getPetitions.IsStarted() || voteZone.IsStarted() || petitions.IsStarted() || earnMoney.IsStarted() || learn.IsStarted()
            || carePackage.IsStarted() || help.IsStarted() || relax.IsStarted() || buy.IsStarted() || invest.IsStarted() || sendMoney.IsStarted() ||
            graphs.IsStarted() || globalInfo.IsStarted());

    }
}
