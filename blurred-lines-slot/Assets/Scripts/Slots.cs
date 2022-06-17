using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlotHelper;
using System.Linq;
public class Slots : MonoBehaviour
{
    public enum SlotState
    {
        Waiting = 0,
        Spinning = 1,
        Auto = 2,
        Winner = 3,
    }

    public SlotState current_slot_state_ = SlotState.Waiting;

    [SerializeField] private Button spin_button_;
    private Animator spin_btn_anime_;
    [Space(5)]
    public Reel[] reels;
    [Space(5)]
    [SerializeField] private List<Symbol> all_symbols_;
    [Space(5)]
    [SerializeField] private List<Text> win_amount_texts_;

    [SerializeField] private GameObject present_win_seq_;

    private bool startSpin;
    private bool is_auto_play_;
    private int num_auto_spins_;
    private bool is_last_auto_spin_forced_;

    private bool is_turbo_spin_enabled_ = false;

    public int GetNumOfAutoSpinsLeft() { return num_auto_spins_; }
    public void SetTurboSpinStatus(bool enabled) { is_turbo_spin_enabled_ = enabled; }

    private List<List<int>> slot_outcome_ = new List<List<int>>();

    private void Awake()
    {
        SetupSlotSymbolsOnReels();
    }

    void Start()
    {
        spin_button_.onClick.AddListener(TryStartSpinning);
        spin_btn_anime_ = spin_button_.GetComponentInChildren<Animator>();

        startSpin = false;
    }

    void Update()
    {
        if (!startSpin)  
        {
            if (Input.GetKeyDown(KeyCode.Space)) //input that starts slot machine
            {
                TryStartSpinning();
            }
        }
    }

    public void TryStartSpinning() // spin callback
    {
        Debug.Log("TryStartSpinning");

        // prevents interference if the reel are still spinning

        if (!startSpin)
        {
            startSpin = true;
            spin_button_.interactable = false;
            spin_btn_anime_.SetTrigger(Consts.spin_button_trigger_);

            current_slot_state_ = SlotState.Spinning;
            Debug.Log("current_slot_state_: SlotState.Spinning");
            slot_outcome_.Clear();

            ClearSlotScreenVfx();

            AudioManager.instance_.PlayWinSound(false);
            AudioManager.instance_.PlaySpinSound();

            StartCoroutine(Spinning(OnNormalSpinStopped));
        }       
    }

    public void TryStartAutoSpins(int num_spins, out bool success)
    {
        if (!startSpin)
        {
            success = true;
            Debug.Log("TryStartAutoSpins");

            current_slot_state_ = SlotState.Auto;
            Debug.Log("current_slot_state_: SlotState.Auto");
            slot_outcome_.Clear();

            ClearSlotScreenVfx();

            AutoSpins(num_spins);
        }
        else { 
            success = false;
        }
    }

    private void ClearSlotScreenVfx()
    {
        // clear win screen
        present_win_seq_.SetActive(false);
        foreach (Reel reel in reels)
        {
            foreach (Transform symbol in reel.reel_symbol_transforms_)
            {
                Animator anime = symbol.GetComponentInChildren<Animator>();
                if (anime) { anime.SetBool("win", false); }
                anime.transform.localRotation = Quaternion.Euler(symbol.localRotation.x, symbol.localRotation.y, 0);
            }
        }

        foreach (Text txt in win_amount_texts_)
        {
            txt.text = "";
        }
    }

    private void AutoSpins(int amount)
    {
        is_auto_play_ = true;

        num_auto_spins_ = amount;
        Debug.Log($"Num of auto spins set to {amount}");
        // conduct for first spin
        num_auto_spins_--;
        startSpin = true;
        spin_button_.interactable = false;
        spin_btn_anime_.SetTrigger(Consts.spin_button_trigger_);

        slot_outcome_.Clear();

        AudioManager.instance_.PlayWinSound(false);
        AudioManager.instance_.PlaySpinSound();

        StartCoroutine(AutoSpinning(OnSpinFinnished));
    }

    // TODO: DRY violation (normal-auto spin almost the same..)
    private IEnumerator DelayNextAutoSpin(System.Action<string> onCompleted, float delay)
    {
        // TODO: maybe start seq of all wins
        yield return new WaitForSeconds(delay);
        onCompleted(null);
    }

    private void OnDelayedAutoSpinFinnished(string errorMessage) //autospin workflow
    {
        if (errorMessage != null)
        {
            Debug.LogError("Was not able to finalize reward: " + errorMessage);
        }

        if (is_auto_play_)
        {
            if (num_auto_spins_ <= 0 || is_last_auto_spin_forced_)
            {
                //stop loop 
                is_auto_play_ = false;
                is_last_auto_spin_forced_ = false;
                num_auto_spins_ = 0;
                spin_button_.interactable = true;
                StopAllCoroutines();
                //emmit stop event
                current_slot_state_ = SlotState.Waiting;
                Debug.Log("current_slot_state_: SlotState.Waiting");
            }
            else
            {
                if (!startSpin)
                {
                    num_auto_spins_--;
                    startSpin = true;
                    spin_button_.interactable = false;
                    spin_btn_anime_.SetTrigger(Consts.spin_button_trigger_);

                    ClearSlotScreenVfx();

                    slot_outcome_.Clear();

                    AudioManager.instance_.PlayWinSound(false);
                    AudioManager.instance_.PlaySpinSound();

                    StartCoroutine(AutoSpinning(OnSpinFinnished));
                }
            }
        }
    }

    private void OnSpinFinnished(string errorMessage) // autospins workflow
    {
        if (errorMessage != null)
        {
            Debug.LogError("Was not able to execute spin: " + errorMessage);
        }

        // determine outcome winning ways
        List<Structs.WinningCombination> winning_ways = GetWinningWays();

        // TODO: calculate payout

        // present win or go to waiting state
        bool WINNER = winning_ways.Count > 0;

        if (WINNER)
        {
            current_slot_state_ = SlotState.Winner;
            Debug.Log("current_slot_state_: SlotState.Winner");

            present_win_seq_.SetActive(true);
            List<int> display_slots = new List<int>() { 0, 1, 2 };

            AudioManager.instance_.PlayWinSound(true);

            foreach (var winways in winning_ways)
            {
                Debug.Log("<color=yellow>W</color><color=red>I</color><color=brown>N</color><color=green>N</color><color=cyan>E</color><color=GRAY>R</color> ----> " +
                    $"SYMBOL {winways.win_symbol_id} PAYS {winways.ways}WAYS x {winways.streak} x Symbol Amount");

                // animate winning text
                int slot = Random.Range(0, display_slots.Count);
                win_amount_texts_[display_slots[slot]].text = $"{winways.ways}Ways x {winways.streak} x {Enums.IdToSymbol(winways.win_symbol_id)}";
                display_slots.RemoveAt(slot);

                // animate winning (visible) symbols till streak over
                foreach (Reel reel in reels)
                {
                    if (reel.id > winways.streak) { break; }

                    foreach (Transform symbol in reel.reel_symbol_transforms_)
                    {
                        if ((symbol.name == Enums.SymbolToString(Enums.IdToSymbol(winways.win_symbol_id))) || (symbol.name == Enums.SymbolToString(Enums.Symbol.Wild) && winways.wilds))
                        {
                            Animator anime = symbol.GetComponentInChildren<Animator>();
                            if (anime) { anime.SetBool("win", true); }
                        }
                    }
                }
            }
        }

        StartCoroutine(DelayNextAutoSpin(OnDelayedAutoSpinFinnished, WINNER ? 2f : 0.5f));
    }

    public void StopAutoSpins(bool force)
    {
        num_auto_spins_ = 0;
        // emmit last spin info
        is_last_auto_spin_forced_ = force;
    }

    IEnumerator Spinning(System.Action<string> onCompleted)
    {
        foreach (Reel spinner in reels)
        {
            //tells each reel to start spinning
            spinner.spin = true;
        }

        SetupSlotSymbolsOnReels();

        for (int i = 0; i < reels.Length; i++)
        {
            //allow the reel to spin for a random amount of time then stop
            yield return new WaitForSeconds(is_turbo_spin_enabled_ ? Random.Range(.5f, .7f) : Random.Range(1,3));
            reels[i].spin = false;
            slot_outcome_.Add(reels[i].SetReelOutcome());
            AudioManager.instance_.PlayReelStopSound();
        }

        onCompleted(null);
    }


    protected List<Structs.WinningCombination> GetWinningWays()
    {
        // track winning symbols and ways for payout
        List<Structs.WinningCombination> winning_ways = new List<Structs.WinningCombination>(); // symbol ID and WAYS

        // all permutations + TODO: wilds
        int wild_index = (int)Enums.SymbolToId(Enums.Symbol.Wild);
        Debug.Log("Wild Index: " + wild_index);

        List<List<Structs.ReelSymbols>> stacked_symbols = new List<List<Structs.ReelSymbols>>();

        for (int i = 0; i < slot_outcome_.Count; i++)
        {
            // foreach reel group symbols by id and occurencess
            var symbol_occurences = slot_outcome_[i].GroupBy(x =>x).Select(x => new { Key = x.Key, total = x.Count()}).ToList();

            List<Structs.ReelSymbols> l_symbols = new List<Structs.ReelSymbols>();
            foreach (var item in symbol_occurences)
            {
                Structs.ReelSymbols reel_symbols = new Structs.ReelSymbols();
                reel_symbols.symbol_id = item.Key;
                reel_symbols.occurences = item.total;

                l_symbols.Add(reel_symbols);
            }

            stacked_symbols.Add(l_symbols);
        }

        // set winning ways

        foreach (var reelSymbol in stacked_symbols[0])
        {

            int symbol_id = reelSymbol.symbol_id;
            int total_ways = reelSymbol.occurences;
            int i_reel = 0; // i_reel >= 2 win req
            bool wilds = false;

            for (int i = 1; i < stacked_symbols.Count; i++)
            {
                bool next_symbol_found = false;

                foreach (var symbol in stacked_symbols[i])
                {
                    if (symbol.symbol_id == symbol_id || symbol.symbol_id == wild_index)
                    {
                        total_ways *= symbol.occurences;
                        i_reel++;
                        next_symbol_found = true;
                        if (!wilds)
                        {
                            wilds = symbol.symbol_id == wild_index;
                        }
                        break;
                    }
                }
                if (!next_symbol_found) { break; }
            }

            Debug.Log($"Symbol ({Enums.IdToSymbol(symbol_id)}) has total ({total_ways}) x Ways");

            if(i_reel >= 2)
            {
                Structs.WinningCombination winning_combo = new Structs.WinningCombination();
                winning_combo.win_symbol_id = symbol_id;
                winning_combo.ways = total_ways;
                winning_combo.streak = i_reel + 1;
                winning_combo.wilds = wilds;

                winning_ways.Add(winning_combo);
            }
        }

        return winning_ways;
    }

    private IEnumerator DelayNextSpin(System.Action<string> onCompleted)
    {
        // TODO: maybe start seq of all wins
        yield return new WaitForSeconds(2f);
        onCompleted(null);
    }

    private void OnDelayedSpinFinnished(string errorMessage)
    {
        if (errorMessage != null)
        {
            Debug.LogError("Was not able to finalize reward: " + errorMessage);
        }

        //allows the machine to be started again
        startSpin = false;
        spin_button_.interactable = true;

        current_slot_state_ = SlotState.Waiting;
        Debug.Log("current_slot_state_: SlotState.Waiting");
    }

    private void OnNormalSpinStopped(string errorMessage) // normal spin flow
    {
        if (errorMessage != null)
        {
            Debug.LogError("Was not able to execute spin: " + errorMessage);
        }

        // determine outcome and winning ways
        List<Structs.WinningCombination> winning_ways = GetWinningWays();

        // TODO: calculate payout

        // present win or go to waiting state
        bool WINNER = winning_ways.Count > 0;

        if (WINNER)
        {
            current_slot_state_ = SlotState.Winner;
            Debug.Log("current_slot_state_: SlotState.Winner");

            present_win_seq_.SetActive(true);
            List<int> display_slots = new List<int>() { 0, 1, 2 };

            AudioManager.instance_.PlayWinSound(true);

            foreach (var winways in winning_ways)
            {
                Debug.Log("<color=yellow>W</color><color=red>I</color><color=brown>N</color><color=green>N</color><color=cyan>E</color><color=GRAY>R</color> ----> " +
                    $"SYMBOL {winways.win_symbol_id} PAYS {winways.ways}WAYS x {winways.streak} x Symbol Amount");

                // animate winning text
                int slot = Random.Range(0, display_slots.Count);
                win_amount_texts_[display_slots[slot]].text = $"{winways.ways}Ways x {winways.streak} x {Enums.IdToSymbol(winways.win_symbol_id)}";
                display_slots.RemoveAt(slot);

                // animate winning (visible) symbols till streak over
                foreach(Reel reel in reels)
                {
                    if(reel.id > winways.streak) { break; }

                    foreach (Transform symbol in reel.reel_symbol_transforms_)
                    {
                        if((symbol.name == Enums.SymbolToString(Enums.IdToSymbol(winways.win_symbol_id))) || (symbol.name == Enums.SymbolToString(Enums.Symbol.Wild) && winways.wilds))
                        {
                            Animator anime = symbol.GetComponentInChildren<Animator>();
                            if (anime) { anime.SetBool("win", true); }
                        }
                    }
                }

            }

            StartCoroutine(DelayNextSpin(OnDelayedSpinFinnished));
        }
        else
        {
            //allows the machine to be started again
            startSpin = false;
            spin_button_.interactable = true;

            current_slot_state_ = SlotState.Waiting;
            Debug.Log("current_slot_state_: SlotState.Waiting");
        }
    }

    private IEnumerator AutoSpinning(System.Action<string> onCompleted)
    {
        foreach (Reel spinner in reels)
        {
            //tells each reel to start spinning
            spinner.spin = true;
        }

        SetupSlotSymbolsOnReels();

        for (int i = 0; i < reels.Length; i++)
        {
            //allow the reel to spin for a random amount of time then stop
            yield return new WaitForSeconds(is_turbo_spin_enabled_ ? Random.Range(.5f, .7f) : Random.Range(1, 3));
            reels[i].spin = false;
            slot_outcome_.Add(reels[i].SetReelOutcome());
            AudioManager.instance_.PlayReelStopSound();
        }
        //allows the machine to be started again
        startSpin = false;

        yield return new WaitForSeconds(1f);
        onCompleted(null);
    }

    private void SetupSlotSymbolsOnReels()
    {
        Debug.Log("Shuffle Reel Symbols");
        // Randomly populate all reels with symbols
        int all_symbol_count = all_symbols_.Count;

        int[] frequency_table = new int[all_symbol_count];
        for (int j = 0; j < all_symbol_count; j++)
        {
            frequency_table.SetValue(all_symbols_[j]._FREQUENCY_, j);
        }

        foreach (Reel reel in reels)
        {
            // randomly take 10 random (TODO: unless scatter - appears only one on reel) symbols and populate reel
            List<Symbol> random_symbols = new List<Symbol>();

            for (int i = 0; i < all_symbol_count; i++)
            {
                int random_symbol_index = GetRandomWeightedIndex(frequency_table);
                // TODO: ugly hack - refactor
                Symbol selected_symbol = all_symbols_[random_symbol_index];
                if (reel.id == 1) {
                    if(selected_symbol._ID_ == (int)Enums.Symbol.Wild)
                    {
                        int random = Random.Range(1, 8);
                        selected_symbol = all_symbols_[random];
                    }
                }
                random_symbols.Add(selected_symbol);
            }
            reel.InitRandomSymbolsOnReel(random_symbols);
        }
    }

    public int GetRandomWeightedIndex(int[] weights)
    {
        // Get the total sum of all the weights.
        int elementCount = weights.Length;
        int weightSum = 0;
        for (int i = 0; i < elementCount; ++i)
        {
            weightSum += weights[i];
        }

        // Step through all the possibilities, one by one, checking to see if each one is selected.     
        int index = 0;
        int lastIndex = elementCount - 1;
        while (index < lastIndex)
        {
            // Do a probability check with a likelihood of weights[index] / weightSum.
            if (Random.Range(0, weightSum) < weights[index])
            {
                return index;
            }

            // Remove the last item from the sum of total untested weights and try again.
            weightSum -= weights[index++];
        }

        // No other item was selected, so return very last index.
        return index;
        
    }

}
