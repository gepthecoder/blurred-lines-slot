using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlotHelper;
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
    [SerializeField] private List<LineVFX> all_vfx_lines_;
    [Space(5)]
    [SerializeField] private List<Text> win_amount_texts_;

    [SerializeField] private GameObject present_win_seq_;

    private bool startSpin;
    private bool is_auto_play_;
    private int num_auto_spins_;
    private bool is_last_auto_spin_forced_;

    public int GetNumOfAutoSpinsLeft() { return num_auto_spins_; }

    private List<Vector3> slot_outcome_ = new List<Vector3>();

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
        foreach (LineVFX lineVfx in all_vfx_lines_)
        {
            lineVfx.DisableLine();
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
        StartCoroutine(AutoSpinning(OnSpinFinnished));
    }

    // TODO: DRY violation
    private IEnumerator DelayNextAutoSpin(System.Action<string> onCompleted, float delay)
    {
        // TODO: maybe start seq of all wins
        yield return new WaitForSeconds(delay);
        onCompleted(null);
    }

    private void OnDelayedAutoSpinFinnished(string errorMessage)
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

        // determine outcome
        List<Line> all_lines = ForgeLines();

        // determine winning lines
        List<Structs.WinningCombination> winning_lines = GetWinningLines(all_lines);

        // TODO: calculate payout

        // present win or go to waiting state
        bool WINNER = winning_lines.Count > 0;

        if (WINNER)
        {
            current_slot_state_ = SlotState.Winner;
            Debug.Log("current_slot_state_: SlotState.Winner");

            present_win_seq_.SetActive(true);

            foreach (var winline in winning_lines)
            {
                Debug.Log("<color=yellow>W</color><color=red>I</color><color=brown>N</color><color=green>N</color><color=cyan>E</color><color=GRAY>R</color> ----> " +
                    $"LINE {winline.line_id} PAYS {winline.occurence} x {Enums.IdToSymbol(winline.symbol)}");
                win_amount_texts_[winline.line_id-1].text = $"{winline.occurence} x {Enums.IdToSymbol(winline.symbol)}";

                foreach (LineVFX lineVfx in all_vfx_lines_)
                {
                    if (lineVfx.line_id_ == winline.line_id)
                    {
                        lineVfx.EnableLine();
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
            yield return new WaitForSeconds(Random.Range(1,3));
            reels[i].spin = false;
            slot_outcome_.Add(reels[i].SetReelOutcome());
        }

        onCompleted(null);
    }

    protected List<Line> ForgeLines()
    {
        Debug.Log($"{(int)slot_outcome_[0].x}{(int)slot_outcome_[1].x}{(int)slot_outcome_[2].x}{(int)slot_outcome_[3].x}{(int)slot_outcome_[4].x}");
        Debug.Log($"{(int)slot_outcome_[0].y}{(int)slot_outcome_[1].y}{(int)slot_outcome_[2].y}{(int)slot_outcome_[3].y}{(int)slot_outcome_[4].y}");
        Debug.Log($"{(int)slot_outcome_[0].z}{(int)slot_outcome_[1].z}{(int)slot_outcome_[2].z}{(int)slot_outcome_[3].z}{(int)slot_outcome_[4].z}");

        Line line1 = new Line(1, (int)slot_outcome_[0].x, (int)slot_outcome_[1].x, (int)slot_outcome_[2].x, (int)slot_outcome_[3].x, (int)slot_outcome_[4].x);
        Line line2 = new Line(2, (int)slot_outcome_[0].y, (int)slot_outcome_[1].y, (int)slot_outcome_[2].y, (int)slot_outcome_[3].y, (int)slot_outcome_[4].y);
        Line line3 = new Line(3, (int)slot_outcome_[0].z, (int)slot_outcome_[1].z, (int)slot_outcome_[2].z, (int)slot_outcome_[3].z, (int)slot_outcome_[4].z);

        List<Line> all_lines = new List<Line>();
        all_lines.Add(line1);
        all_lines.Add(line2);
        all_lines.Add(line3);

        return all_lines;
    }

    protected List<Structs.WinningCombination> GetWinningLines(List<Line> all_lines)
    {
        // track lines with symbols for payout
        List<Structs.WinningCombination> winning_lines = new List<Structs.WinningCombination>(); // line ID and WIN (symbol, occurences)

        // straight lines + wilds
        int wild_index = (int)Enums.SymbolToId(Enums.Symbol.Wild);
        Debug.Log("Wild Index: " + wild_index);

        int occurences_in_row = 1; //one is always
        int winning_symbol_id = -1;
        int i_win_symbol;
        int i_win_symbol_index = 0;

        bool is_wild_line = false;

        for (int i = 0; i < all_lines.Count; i++)
        {
            int i_line_count = all_lines[i].i_line.Count;
            i_win_symbol_index = 0;

            for (int j = 0; j < i_line_count; j++)
            {
                if (j + 1 >= i_line_count) {
                    //Debug.Log($"Break❌: Line {all_lines[i].ID}");
                    break;
                }

                i_win_symbol = all_lines[i].i_line[i_win_symbol_index];
                if(i_win_symbol == wild_index) { 
                    if(i_win_symbol_index < i_line_count)
                    {
                        i_win_symbol_index++;
                        i_win_symbol = all_lines[i].i_line[i_win_symbol_index];
                    }
                }
                //Debug.Log($"i_win_symbol: Line {all_lines[i].ID} has current winning symb ({i_win_symbol}) index:({i_win_symbol_index})");

                if ((all_lines[i].i_line[j] == all_lines[i].i_line[j + 1]) 
                    || (j == 0 && (all_lines[i].i_line[j] == wild_index)) 
                    || (all_lines[i].i_line[j + 1] == wild_index) 
                    || (all_lines[i].i_line[j + 1] == i_win_symbol))
                {
                    occurences_in_row++;
                    //Debug.Log($"occurences_in_row++ ({occurences_in_row}): Line {all_lines[i].ID} has current winning symb ({i_win_symbol}) index:({i_win_symbol_index})");

                    if (occurences_in_row >= 3)
                    {
                        //full line
                        if (occurences_in_row == 5)
                        {
                            // wild line check
                            is_wild_line = ((all_lines[i].i_line[0] == wild_index)
                                && (all_lines[i].i_line[0] == all_lines[i].i_line[1])
                                && (all_lines[i].i_line[1] == all_lines[i].i_line[2])
                                && (all_lines[i].i_line[2] == all_lines[i].i_line[3])
                                && (all_lines[i].i_line[3] == all_lines[i].i_line[4]));
                            break;
                        }

                        winning_symbol_id = is_wild_line ? wild_index : all_lines[i].i_line[0];
                    }
                }
                else { break; }
            }

            // we have a win
            if (occurences_in_row >= 3)
            {
                //Debug.Log($"we have a win.. is wild line:  " + is_wild_line);

                // check wilds as winning symbol
                if (winning_symbol_id == wild_index && !is_wild_line)
                {
                    List<int> without_wilds = all_lines[i].i_line;
                    without_wilds.RemoveAll(item => item == wild_index);
                    winning_symbol_id = without_wilds[0];
                }
                Structs.WinningCombination win = new Structs.WinningCombination();
                win.symbol = winning_symbol_id;
                win.occurence = occurences_in_row;
                win.line_id = all_lines[i].ID;
                winning_lines.Add(win);
            }
            else
            {
                Debug.Log($"Line{all_lines[i].ID} has ({occurences_in_row}) occurences in row.");
            }

            occurences_in_row = 1;
        }

        return winning_lines;
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

    private void OnNormalSpinStopped(string errorMessage)
    {
        if (errorMessage != null)
        {
            Debug.LogError("Was not able to execute spin: " + errorMessage);
        }

        // determine outcome
        List<Line> all_lines = ForgeLines();

        // determine winning lines
        List<Structs.WinningCombination> winning_lines = GetWinningLines(all_lines);

        // TODO: calculate payout

        // present win or go to waiting state
        bool WINNER = winning_lines.Count > 0;

        if (WINNER)
        {
            current_slot_state_ = SlotState.Winner;
            Debug.Log("current_slot_state_: SlotState.Winner");
            
            present_win_seq_.SetActive(true);

            foreach (var winline in winning_lines)
            {
                Debug.Log("<color=yellow>W</color><color=red>I</color><color=brown>N</color><color=green>N</color><color=cyan>E</color><color=GRAY>R</color> ----> " +
                    $"LINE {winline.line_id} PAYS {winline.occurence} x {Enums.IdToSymbol(winline.symbol)}");
                win_amount_texts_[winline.line_id-1].text = $"{winline.occurence} x {Enums.IdToSymbol(winline.symbol)}";

                foreach (LineVFX lineVfx in all_vfx_lines_)
                {
                    if(lineVfx.line_id_ == winline.line_id)
                    {
                        lineVfx.EnableLine();
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
            yield return new WaitForSeconds(Random.Range(1, 3));
            reels[i].spin = false;
            slot_outcome_.Add(reels[i].SetReelOutcome());
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
                random_symbols.Add(all_symbols_[random_symbol_index]);
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

[System.Serializable]
public class Line
{
    public int ID;
    public int x, y, z, w, q;

    public Line(int id, int x_, int y_, int z_, int w_, int q_) {
        ID = id;
        x = x_;
        y = y_;
        z = z_;
        w = w_;
        q = q_;

        i_line.Add(x); i_line.Add(y); i_line.Add(z); i_line.Add(w); i_line.Add(q);
    }

    public Line() { }

    public List<int> i_line = new List<int>();
}
