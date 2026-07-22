using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class LevelDifficultyCalculator
{
    public class DifficultyResult
    {
        public float TotalScore; // 0 - 100
        public string Category;   // Rất dễ, Dễ, Trung bình, Khó, Chuyên gia
        public Color CategoryColor;

        public float X1_MoveTightness;      // 0.30
        public float X2_ConditionComplexity; // 0.20
        public float X3_ConstraintConflict;  // 0.30
        public float X4_GridConstraint;      // 0.10
        public float X5_ResourceScarcity;    // 0.10

        public int M_opt;
        public int MoveLimit;
        public int Slack;
        public int N_person;
        public int N_correction;
        public bool IsSolvable = true;
        public bool IsApproximate = false;

        public int ConflictPairs;
        public int SharedContention;
        public int ChainDepth;

        public float C_avg; // Average condition complexity per person

        public float BlockedRatio;
        public float AvgNeighbor;
        public int TightSeatCount;

        public int WaitLineCapacity;
    }

    public class PersonInfo
    {
        public int Index;
        public string Name;
        public PersonDataSO Data;
        public ConditionsSO Conditions;
        public Seat SeatAsset;
    }

    public class BoardSeatInfo
    {
        public int Index;
        public int X;
        public int Y;
        public int NeighborCount;
        public List<CellDataSO> NeighborCells = new List<CellDataSO>();
        public CellDataSO CurrentCellData;
    }

    public static DifficultyResult Calculate(LevelConfig levelConfig)
    {
        DifficultyResult result = new DifficultyResult();
        if (levelConfig == null) return result;

        result.MoveLimit = levelConfig.MoveLimit;
        GridConfig boardGrid = levelConfig.BoardGrid;
        GridConfig waitGrid = levelConfig.WaitLineGrid;

        // 1. Extract Persons & Board Seats
        List<PersonInfo> persons = ExtractPersons(levelConfig);
        result.N_person = persons.Count;

        List<BoardSeatInfo> boardSeats = ExtractBoardSeats(boardGrid);
        result.WaitLineCapacity = waitGrid != null ? waitGrid.Size.x * waitGrid.Size.y : 0;

        // 2. X1: Move Tightness (0.30)
        CalculateX1(result, persons, boardSeats, boardGrid);

        // 3. X2: Condition Complexity (0.20)
        CalculateX2(result, persons);

        // 4. X3: Constraint Conflict (0.30)
        CalculateX3(result, persons, boardSeats, boardGrid);

        // 5. X4: Grid Constraint (0.10)
        CalculateX4(result, persons, boardSeats, boardGrid);

        // 6. X5: Resource Scarcity (0.10)
        CalculateX5(result, persons, boardGrid);

        // 7. Overall Score & Category (5 Components)
        result.TotalScore = 100.0f * (
            0.30f * result.X1_MoveTightness +
            0.20f * result.X2_ConditionComplexity +
            0.30f * result.X3_ConstraintConflict +
            0.10f * result.X4_GridConstraint +
            0.10f * result.X5_ResourceScarcity
        );

        result.TotalScore = Mathf.Clamp(result.TotalScore, 0f, 100f);

        if (!result.IsSolvable)
        {
            result.Category = "Không thể giải (MoveLimit < M_opt)";
            result.CategoryColor = new Color(0.85f, 0.2f, 0.2f);
        }
        else if (result.TotalScore <= 20f)
        {
            result.Category = "Rất dễ (Tutorial)";
            result.CategoryColor = new Color(0.18f, 0.8f, 0.44f); // Green
        }
        else if (result.TotalScore <= 40f)
        {
            result.Category = "Dễ";
            result.CategoryColor = new Color(0.2f, 0.6f, 0.86f); // Blue
        }
        else if (result.TotalScore <= 60f)
        {
            result.Category = "Trung bình";
            result.CategoryColor = new Color(0.95f, 0.77f, 0.06f); // Yellow
        }
        else if (result.TotalScore <= 80f)
        {
            result.Category = "Khó";
            result.CategoryColor = new Color(0.9f, 0.49f, 0.13f); // Orange
        }
        else
        {
            result.Category = "Chuyên gia";
            result.CategoryColor = new Color(0.91f, 0.3f, 0.24f); // Red
        }

        return result;
    }

    #region Person & Seat Extraction
    private static List<PersonInfo> ExtractPersons(LevelConfig levelConfig)
    {
        List<PersonInfo> persons = new List<PersonInfo>();
        HashSet<Person> visitedPrefabs = new HashSet<Person>();

        CollectPersonsFromGrid(levelConfig.WaitLineGrid, persons, visitedPrefabs);
        CollectPersonsFromGrid(levelConfig.BoardGrid, persons, visitedPrefabs);

        return persons;
    }

    private static void CollectPersonsFromGrid(GridConfig grid, List<PersonInfo> list, HashSet<Person> visitedPrefabs)
    {
        if (grid == null || grid.BaseGrid == null) return;
        for (int y = 0; y < grid.Size.y; y++)
        {
            if (grid.BaseGrid[y] == null || grid.BaseGrid[y].Values == null) continue;
            for (int x = 0; x < grid.Size.x; x++)
            {
                CellDataSO cellData = grid.BaseGrid[y].Values[x];
                if (cellData is Seat seat && seat.DefaultPerson != null)
                {
                    Person personPrefab = seat.DefaultPerson;
                    if (visitedPrefabs.Contains(personPrefab)) continue;
                    visitedPrefabs.Add(personPrefab);

                    SerializedObject personSo = new SerializedObject(personPrefab);
                    PersonDataSO data = personSo.FindProperty("data")?.objectReferenceValue as PersonDataSO;
                    ConditionsSO conds = personSo.FindProperty("conditions")?.objectReferenceValue as ConditionsSO;

                    list.Add(new PersonInfo
                    {
                        Index = list.Count,
                        Name = data != null ? data.Name : personPrefab.name,
                        Data = data,
                        Conditions = conds,
                        SeatAsset = seat
                    });
                }
            }
        }
    }

    private static List<BoardSeatInfo> ExtractBoardSeats(GridConfig boardGrid)
    {
        List<BoardSeatInfo> seats = new List<BoardSeatInfo>();
        if (boardGrid == null || boardGrid.BaseGrid == null) return seats;

        int width = boardGrid.Size.x;
        int height = boardGrid.Size.y;

        for (int y = 0; y < height; y++)
        {
            if (boardGrid.BaseGrid[y] == null || boardGrid.BaseGrid[y].Values == null) continue;
            for (int x = 0; x < width; x++)
            {
                CellDataSO cell = boardGrid.BaseGrid[y].Values[x];
                if (cell != null && cell.Type == CellType.Seat)
                {
                    BoardSeatInfo info = new BoardSeatInfo
                    {
                        Index = seats.Count,
                        X = x,
                        Y = y,
                        CurrentCellData = cell
                    };

                    int count = 0;
                    int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
                    int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

                    for (int i = 0; i < 8; i++)
                    {
                        int nx = x + dx[i];
                        int ny = y + dy[i];
                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            if (boardGrid.BaseGrid[ny] != null && boardGrid.BaseGrid[ny].Values != null)
                            {
                                CellDataSO nCell = boardGrid.BaseGrid[ny].Values[nx];
                                if (nCell != null && nCell.Type != CellType.Block)
                                {
                                    count++;
                                    info.NeighborCells.Add(nCell);
                                }
                            }
                        }
                    }

                    info.NeighborCount = count;
                    seats.Add(info);
                }
            }
        }

        return seats;
    }
    #endregion

    #region X1: Move Tightness
    private static void CalculateX1(DifficultyResult res, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        if (persons.Count == 0)
        {
            res.M_opt = 0;
            res.Slack = res.MoveLimit;
            res.X1_MoveTightness = 0f;
            res.N_correction = 0;
            res.IsSolvable = true;
            return;
        }

        if (boardSeats.Count < persons.Count)
        {
            res.M_opt = 999;
            res.IsSolvable = false;
            res.X1_MoveTightness = 1.0f;
            return;
        }

        // Run solver to find minimum moves M_opt
        bool approximate = false;
        int mOpt = SolveOptimalMoves(persons, boardSeats, boardGrid, out approximate);

        res.M_opt = mOpt;
        res.IsApproximate = approximate;
        res.N_correction = Mathf.Max(0, mOpt - persons.Count);
        res.Slack = res.MoveLimit - mOpt;

        if (res.Slack < 0)
        {
            res.IsSolvable = false;
            res.X1_MoveTightness = 1.0f;
        }
        else
        {
            res.IsSolvable = true;
            res.X1_MoveTightness = Mathf.Clamp01(1.0f - (float)res.Slack / Mathf.Max(1, mOpt));
        }
    }

    private static int SolveOptimalMoves(List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid, out bool isApproximate)
    {
        isApproximate = false;
        int nPerson = persons.Count;
        int nSeat = boardSeats.Count;

        // If person count is small, run IDA* search
        if (nPerson <= 8 && nSeat <= 12)
        {
            int idaResult = RunIDAStarSearch(persons, boardSeats, boardGrid);
            if (idaResult > 0 && idaResult < 99)
            {
                return idaResult;
            }
        }

        // Fallback: Greedy Assignment + Local Repair
        isApproximate = true;
        return SolveGreedyLocalRepair(persons, boardSeats, boardGrid);
    }

    private static int RunIDAStarSearch(List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        int nP = persons.Count;
        int nS = boardSeats.Count;

        // State representation: int[] assignment of length nP (-1 for outside, 0..nS-1 for seat)
        int[] initialAssignment = new int[nP];
        for (int i = 0; i < nP; i++) initialAssignment[i] = -1;

        int bound = CalculateUnhappyCount(initialAssignment, persons, boardSeats, boardGrid);
        int maxBudget = nP + 10;
        int stateVisitedCount = 0;

        while (bound <= maxBudget && stateVisitedCount < 20000)
        {
            HashSet<string> visited = new HashSet<string>();
            int nextBound = DepthLimitedSearch(initialAssignment, 0, bound, persons, boardSeats, boardGrid, visited, ref stateVisitedCount);
            if (nextBound < 0)
            {
                return -nextBound; // Found optimal moves
            }
            if (nextBound >= 999) break;
            bound = nextBound;
        }

        return SolveGreedyLocalRepair(persons, boardSeats, boardGrid);
    }

    private static int DepthLimitedSearch(int[] state, int g, int bound, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid, HashSet<string> visited, ref int stateVisitedCount)
    {
        stateVisitedCount++;
        int h = CalculateUnhappyCount(state, persons, boardSeats, boardGrid);
        int f = g + h;
        if (f > bound) return f;
        if (h == 0) return -g; // Goal found!

        int minExceed = 999;
        string key = string.Join(",", state);
        if (visited.Contains(key)) return 999;
        visited.Add(key);

        int nP = persons.Count;
        int nS = boardSeats.Count;

        // Try Place Actions
        for (int p = 0; p < nP; p++)
        {
            if (state[p] == -1) // Person is outside
            {
                for (int s = 0; s < nS; s++)
                {
                    bool seatOccupied = false;
                    for (int other = 0; other < nP; other++)
                    {
                        if (state[other] == s) { seatOccupied = true; break; }
                    }

                    if (!seatOccupied)
                    {
                        int[] child = (int[])state.Clone();
                        child[p] = s;
                        int res = DepthLimitedSearch(child, g + 1, bound, persons, boardSeats, boardGrid, visited, ref stateVisitedCount);
                        if (res < 0) return res;
                        minExceed = Mathf.Min(minExceed, res);
                    }
                }
            }
        }

        // Try Move / Swap Actions if already seated
        for (int p = 0; p < nP; p++)
        {
            if (state[p] != -1)
            {
                for (int s = 0; s < nS; s++)
                {
                    if (state[p] == s) continue;
                    int occupant = -1;
                    for (int other = 0; other < nP; other++)
                    {
                        if (state[other] == s) { occupant = other; break; }
                    }

                    int[] child = (int[])state.Clone();
                    if (occupant == -1)
                    {
                        child[p] = s; // Move to empty seat
                    }
                    else
                    {
                        // Swap
                        child[p] = s;
                        child[occupant] = state[p];
                    }

                    int res = DepthLimitedSearch(child, g + 1, bound, persons, boardSeats, boardGrid, visited, ref stateVisitedCount);
                    if (res < 0) return res;
                    minExceed = Mathf.Min(minExceed, res);
                }
            }
        }

        return minExceed;
    }

    private static int SolveGreedyLocalRepair(List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        int nP = persons.Count;
        int nS = boardSeats.Count;
        int[] state = new int[nP];
        for (int i = 0; i < nP; i++) state[i] = -1;

        int nPlaceMoves = 0;

        // Sort persons: Cell -> Dish -> Person
        var sortedPersons = persons.OrderBy(p => GetPersonPriority(p)).ToList();

        HashSet<int> usedSeats = new HashSet<int>();
        foreach (var p in sortedPersons)
        {
            int bestSeat = -1;
            int bestScore = -1;

            for (int s = 0; s < nS; s++)
            {
                if (usedSeats.Contains(s)) continue;
                state[p.Index] = s;
                bool isHappy = EvaluatePersonConditionAtState(p, state, persons, boardSeats, boardGrid);
                int score = isHappy ? 10 : (boardSeats[s].NeighborCount);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestSeat = s;
                }
                state[p.Index] = -1;
            }

            if (bestSeat != -1)
            {
                state[p.Index] = bestSeat;
                usedSeats.Add(bestSeat);
                nPlaceMoves++;
            }
            else
            {
                // Assign to first available
                for (int s = 0; s < nS; s++)
                {
                    if (!usedSeats.Contains(s))
                    {
                        state[p.Index] = s;
                        usedSeats.Add(s);
                        nPlaceMoves++;
                        break;
                    }
                }
            }
        }

        // Local Repair: try Swaps to maximize happy count
        int nRepairMoves = 0;
        int maxRepairIter = nP * 2;
        int iter = 0;

        while (iter < maxRepairIter)
        {
            iter++;
            bool improved = false;
            int currentUnhappy = CalculateUnhappyCount(state, persons, boardSeats, boardGrid);
            if (currentUnhappy == 0) break;

            for (int i = 0; i < nP; i++)
            {
                for (int j = i + 1; j < nP; j++)
                {
                    if (state[i] != -1 && state[j] != -1)
                    {
                        // Swap
                        int tmp = state[i];
                        state[i] = state[j];
                        state[j] = tmp;

                        int newUnhappy = CalculateUnhappyCount(state, persons, boardSeats, boardGrid);
                        if (newUnhappy < currentUnhappy)
                        {
                            currentUnhappy = newUnhappy;
                            nRepairMoves++;
                            improved = true;
                            break;
                        }
                        else
                        {
                            // Revert
                            tmp = state[i];
                            state[i] = state[j];
                            state[j] = tmp;
                        }
                    }
                }
                if (improved) break;
            }

            if (!improved) break;
        }

        return nPlaceMoves + nRepairMoves;
    }

    private static int GetPersonPriority(PersonInfo p)
    {
        if (p.Conditions == null) return 0;
        List<SingleConditionsSO> singles = FlattenConditions(p.Conditions);
        if (singles.Any(s => s.FilterTarget == Target.Cell)) return 1;
        if (singles.Any(s => s.FilterTarget == Target.Dish)) return 2;
        return 3; // Person target
    }

    private static int CalculateUnhappyCount(int[] state, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        int unhappy = 0;
        for (int i = 0; i < persons.Count; i++)
        {
            if (state[i] == -1)
            {
                unhappy++;
            }
            else
            {
                if (!EvaluatePersonConditionAtState(persons[i], state, persons, boardSeats, boardGrid))
                {
                    unhappy++;
                }
            }
        }
        return unhappy;
    }

    private static bool EvaluatePersonConditionAtState(PersonInfo person, int[] state, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        if (person.Conditions == null) return true;
        int seatIdx = state[person.Index];
        if (seatIdx == -1) return false;

        BoardSeatInfo currentSeat = boardSeats[seatIdx];

        // Construct 8-way adjacent cell states for condition evaluation
        return EvaluateConditionTree(person.Conditions, currentSeat, state, persons, boardSeats, boardGrid);
    }

    private static bool EvaluateConditionTree(ConditionsSO cond, BoardSeatInfo currentSeat, int[] state, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        if (cond is SingleConditionsSO single)
        {
            return EvaluateSingleCondition(single, currentSeat, state, persons, boardSeats, boardGrid);
        }
        if (cond is CompositeConditionsSO composite)
        {
            if (composite.SubConditions == null || composite.SubConditions.Count == 0) return true;
            if (composite.LogicalOperator == LogicalOperator.And)
            {
                return composite.SubConditions.All(sub => sub == null || EvaluateConditionTree(sub, currentSeat, state, persons, boardSeats, boardGrid));
            }
            else
            {
                return composite.SubConditions.Any(sub => sub != null && EvaluateConditionTree(sub, currentSeat, state, persons, boardSeats, boardGrid));
            }
        }
        return true;
    }

    private static bool EvaluateSingleCondition(SingleConditionsSO single, BoardSeatInfo currentSeat, int[] state, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        int matchCount = 0;
        int width = boardGrid.Size.x;
        int height = boardGrid.Size.y;

        if (single.Scope == Scope.Self)
        {
            if (EvaluateCellMatch(currentSeat.X, currentSeat.Y, currentSeat.CurrentCellData, single, state, persons, boardSeats, boardGrid))
            {
                matchCount = 1;
            }
        }
        else // Adjacent
        {
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nx = currentSeat.X + dx[i];
                int ny = currentSeat.Y + dy[i];
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    CellDataSO nCell = boardGrid.BaseGrid[ny].Values[nx];
                    if (EvaluateCellMatch(nx, ny, nCell, single, state, persons, boardSeats, boardGrid))
                    {
                        matchCount++;
                    }
                }
            }
        }

        return single.Comparator switch
        {
            Comparator.Exact => matchCount == single.Value,
            Comparator.AtLeast => matchCount >= single.Value,
            Comparator.AtMost => matchCount <= single.Value,
            _ => false
        };
    }

    private static bool EvaluateCellMatch(int x, int y, CellDataSO cell, SingleConditionsSO single, int[] state, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        if (cell == null || cell.Type == CellType.Block) return false;

        if (single.FilterTarget == Target.Cell)
        {
            if (single.Filter.Column != -1 && x != single.Filter.Column) return false;
            if (single.Filter.Row != -1 && y != single.Filter.Row) return false;
            return true;
        }

        if (single.FilterTarget == Target.Dish)
        {
            if (cell.Type != CellType.Dish) return false;
            if (cell is Dishes dish)
            {
                if (single.Filter.FoodTags != null && single.Filter.FoodTags.Count > 0)
                {
                    foreach (var requiredFood in single.Filter.FoodTags)
                    {
                        if (!dish.Tags.Contains(requiredFood)) return false;
                    }
                }
                return true;
            }
            return false;
        }

        if (single.FilterTarget == Target.Person)
        {
            if (cell.Type != CellType.Seat) return false;
            // Check if there is a person seated at this seat in the state
            int seatIdx = boardSeats.FindIndex(s => s.X == x && s.Y == y);
            if (seatIdx == -1) return false;

            int seatedPersonIdx = -1;
            for (int p = 0; p < state.Length; p++)
            {
                if (state[p] == seatIdx) { seatedPersonIdx = p; break; }
            }

            if (seatedPersonIdx == -1) return false;
            PersonInfo seatedPerson = persons[seatedPersonIdx];

            if (!string.IsNullOrEmpty(single.Filter.Name))
            {
                if (seatedPerson.Name != single.Filter.Name) return false;
            }

            if (single.Filter.TraitTags != null && single.Filter.TraitTags.Count > 0)
            {
                if (seatedPerson.Data == null || seatedPerson.Data.Trait == null) return false;
                foreach (var trait in single.Filter.TraitTags)
                {
                    if (!seatedPerson.Data.Trait.Contains(trait)) return false;
                }
            }

            return true;
        }

        return false;
    }
    #endregion

    #region X2: Condition Complexity
    private static void CalculateX2(DifficultyResult res, List<PersonInfo> persons)
    {
        if (persons.Count == 0)
        {
            res.X2_ConditionComplexity = 0f;
            return;
        }

        float totalScore = 0f;
        foreach (var p in persons)
        {
            totalScore += ScoreCondition(p.Conditions);
        }

        float cAvg = totalScore / persons.Count;
        const float C_MAX = 5.0f;
        res.C_avg = cAvg;
        res.X2_ConditionComplexity = Mathf.Clamp01(cAvg / C_MAX);
    }

    private static float ScoreCondition(ConditionsSO cond)
    {
        if (cond == null) return 0f;

        if (cond is SingleConditionsSO single)
        {
            float wScope = single.Scope == Scope.Self ? 1.0f : 2.0f;
            float wTarget = single.FilterTarget switch
            {
                Target.Cell => 1.0f,
                Target.Dish => 1.5f,
                Target.Person => 2.5f,
                _ => 1.0f
            };

            float wValue = single.Comparator switch
            {
                Comparator.AtLeast => Mathf.Clamp01((float)single.Value / 8.0f),
                Comparator.Exact => single.Value == 0 ? 0.6f : 0.8f,
                Comparator.AtMost => 0.6f / (1.0f + single.Value),
                _ => 0.5f
            };

            return wScope * wTarget * wValue;
        }

        if (cond is CompositeConditionsSO composite)
        {
            if (composite.SubConditions == null || composite.SubConditions.Count == 0) return 0f;

            List<float> subScores = composite.SubConditions.Select(ScoreCondition).ToList();
            if (composite.LogicalOperator == LogicalOperator.And)
            {
                return subScores.Sum();
            }
            else
            {
                return 0.7f * (subScores.Count > 0 ? subScores.Min() : 0f);
            }
        }

        return 0f;
    }
    #endregion

    #region X3: Constraint Conflict
    private static void CalculateX3(DifficultyResult res, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        if (persons.Count == 0)
        {
            res.X3_ConstraintConflict = 0f;
            return;
        }

        int conflictPairs = 0;
        int sharedContention = 0;

        Dictionary<string, int> demandByTag = new Dictionary<string, int>();
        Dictionary<string, int> demandByPerson = new Dictionary<string, int>();

        // Build edges
        for (int i = 0; i < persons.Count; i++)
        {
            PersonInfo pA = persons[i];
            List<SingleConditionsSO> singlesA = FlattenConditions(pA.Conditions);

            for (int j = 0; j < persons.Count; j++)
            {
                if (i == j) continue;
                PersonInfo pB = persons[j];
                List<SingleConditionsSO> singlesB = FlattenConditions(pB.Conditions);

                bool aAttractsB = singlesA.Any(c => c.Scope == Scope.Adjacent && c.FilterTarget == Target.Person &&
                    (c.Comparator == Comparator.AtLeast || c.Comparator == Comparator.Exact) && c.Value > 0 &&
                    (string.IsNullOrEmpty(c.Filter.Name) || c.Filter.Name == pB.Name));

                bool bRepelsA = singlesB.Any(c => c.Scope == Scope.Adjacent && c.FilterTarget == Target.Person &&
                    ((c.Comparator == Comparator.Exact && c.Value == 0) || c.Comparator == Comparator.AtMost) &&
                    (string.IsNullOrEmpty(c.Filter.Name) || c.Filter.Name == pA.Name));

                if (aAttractsB && bRepelsA)
                {
                    conflictPairs++;
                }
            }

            // Demand for Dish tags & Persons
            foreach (var c in singlesA)
            {
                if (c.Scope == Scope.Adjacent && c.Value > 0)
                {
                    if (c.FilterTarget == Target.Dish && c.Filter.FoodTags != null)
                    {
                        foreach (var tag in c.Filter.FoodTags)
                        {
                            string tagKey = tag.ToString();
                            demandByTag[tagKey] = demandByTag.GetValueOrDefault(tagKey) + c.Value;
                        }
                    }
                    if (c.FilterTarget == Target.Person && !string.IsNullOrEmpty(c.Filter.Name))
                    {
                        demandByPerson[c.Filter.Name] = demandByPerson.GetValueOrDefault(c.Filter.Name) + c.Value;
                    }
                }
            }
        }

        // Shared Contention
        foreach (var kvp in demandByTag)
        {
            int supply = GetSupplyForFoodTag(kvp.Key, boardGrid, boardSeats);
            if (kvp.Value > supply)
            {
                sharedContention += (kvp.Value - supply);
            }
        }

        foreach (var kvp in demandByPerson)
        {
            int supply = 4; // Approx neighbor slots for person
            if (kvp.Value > supply)
            {
                sharedContention += (kvp.Value - supply);
            }
        }

        // Chain Depth
        int chainDepth = CalculateChainDepth(persons);

        res.ConflictPairs = conflictPairs;
        res.SharedContention = sharedContention;
        res.ChainDepth = chainDepth;

        float score = (0.4f * conflictPairs + 0.4f * sharedContention + 0.2f * chainDepth) / Mathf.Max(1, persons.Count);
        res.X3_ConstraintConflict = Mathf.Clamp01(score);
    }

    private static int GetSupplyForFoodTag(string tagStr, GridConfig boardGrid, List<BoardSeatInfo> boardSeats)
    {
        if (boardGrid == null || boardGrid.BaseGrid == null) return 0;
        int supplySlots = 0;

        for (int y = 0; y < boardGrid.Size.y; y++)
        {
            if (boardGrid.BaseGrid[y] == null || boardGrid.BaseGrid[y].Values == null) continue;
            for (int x = 0; x < boardGrid.Size.x; x++)
            {
                CellDataSO cell = boardGrid.BaseGrid[y].Values[x];
                if (cell is Dishes dish)
                {
                    if (dish.Tags != null && dish.Tags.Any(t => t.ToString() == tagStr))
                    {
                        // Add neighbor seats of this dish
                        supplySlots += 4;
                    }
                }
            }
        }
        return supplySlots;
    }

    private static int CalculateChainDepth(List<PersonInfo> persons)
    {
        int maxDepth = 0;
        for (int i = 0; i < persons.Count; i++)
        {
            List<SingleConditionsSO> singles = FlattenConditions(persons[i].Conditions);
            int depth = singles.Count(c => c.Scope == Scope.Adjacent && c.FilterTarget == Target.Person && c.Value > 0);
            maxDepth = Mathf.Max(maxDepth, depth);
        }
        return maxDepth;
    }

    private static List<SingleConditionsSO> FlattenConditions(ConditionsSO cond)
    {
        List<SingleConditionsSO> list = new List<SingleConditionsSO>();
        if (cond is SingleConditionsSO single)
        {
            list.Add(single);
        }
        else if (cond is CompositeConditionsSO composite && composite.SubConditions != null)
        {
            foreach (var sub in composite.SubConditions)
            {
                list.AddRange(FlattenConditions(sub));
            }
        }
        return list;
    }
    #endregion

    #region X4: Grid Constraint
    private static void CalculateX4(DifficultyResult res, List<PersonInfo> persons, List<BoardSeatInfo> boardSeats, GridConfig boardGrid)
    {
        if (boardGrid == null || boardGrid.BaseGrid == null)
        {
            res.X4_GridConstraint = 0f;
            return;
        }

        int totalCells = boardGrid.Size.x * boardGrid.Size.y;
        int blockedCount = 0;

        for (int y = 0; y < boardGrid.Size.y; y++)
        {
            if (boardGrid.BaseGrid[y] == null || boardGrid.BaseGrid[y].Values == null) continue;
            for (int x = 0; x < boardGrid.Size.x; x++)
            {
                CellDataSO cell = boardGrid.BaseGrid[y].Values[x];
                if (cell != null && cell.Type == CellType.Block)
                {
                    blockedCount++;
                }
            }
        }

        float blockedRatio = (float)blockedCount / Mathf.Max(1, totalCells);
        float avgNeighbor = boardSeats.Count > 0 ? (float)boardSeats.Average(s => s.NeighborCount) : 8.0f;

        int tightSeatCount = 0;
        foreach (var p in persons)
        {
            List<SingleConditionsSO> singles = FlattenConditions(p.Conditions);
            int maxReqAdjacent = singles.Where(c => c.Scope == Scope.Adjacent).Select(c => c.Value).DefaultIfEmpty(0).Max();

            if (maxReqAdjacent > 0)
            {
                if (boardSeats.Any(s => s.NeighborCount < maxReqAdjacent))
                {
                    tightSeatCount++;
                }
            }
        }

        res.BlockedRatio = blockedRatio;
        res.AvgNeighbor = avgNeighbor;
        res.TightSeatCount = tightSeatCount;

        float score = 0.5f * blockedRatio + 0.3f * (1.0f - avgNeighbor / 8.0f) + 0.2f * ((float)tightSeatCount / Mathf.Max(1, persons.Count));
        res.X4_GridConstraint = Mathf.Clamp01(score);
    }
    #endregion

    #region X5: Resource Scarcity
    private static void CalculateX5(DifficultyResult res, List<PersonInfo> persons, GridConfig boardGrid)
    {
        if (persons.Count == 0 || boardGrid == null || boardGrid.BaseGrid == null)
        {
            res.X5_ResourceScarcity = 0f;
            return;
        }

        Dictionary<string, int> demandByTag = new Dictionary<string, int>();
        Dictionary<string, int> supplyByTag = new Dictionary<string, int>();

        foreach (var p in persons)
        {
            List<SingleConditionsSO> singles = FlattenConditions(p.Conditions);
            foreach (var c in singles)
            {
                if (c.FilterTarget == Target.Dish && c.Filter.FoodTags != null && c.Value > 0)
                {
                    foreach (var tag in c.Filter.FoodTags)
                    {
                        string tagKey = tag.ToString();
                        demandByTag[tagKey] = demandByTag.GetValueOrDefault(tagKey) + c.Value;
                    }
                }
            }
        }

        for (int y = 0; y < boardGrid.Size.y; y++)
        {
            if (boardGrid.BaseGrid[y] == null || boardGrid.BaseGrid[y].Values == null) continue;
            for (int x = 0; x < boardGrid.Size.x; x++)
            {
                CellDataSO cell = boardGrid.BaseGrid[y].Values[x];
                if (cell is Dishes dish && dish.Tags != null)
                {
                    foreach (var tag in dish.Tags)
                    {
                        string tagKey = tag.ToString();
                        supplyByTag[tagKey] = supplyByTag.GetValueOrDefault(tagKey) + 1;
                    }
                }
            }
        }

        List<float> scarcityList = new List<float>();
        foreach (var kvp in demandByTag)
        {
            int supply = supplyByTag.GetValueOrDefault(kvp.Key, 0);
            scarcityList.Add((float)kvp.Value / Mathf.Max(1, supply));
        }

        const float SCARCITY_MAX = 2.0f;
        res.X5_ResourceScarcity = scarcityList.Count > 0 ? Mathf.Clamp01(scarcityList.Average() / SCARCITY_MAX) : 0f;
    }
    #endregion
}
