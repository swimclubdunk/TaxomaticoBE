using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Benis : MonoBehaviour
{
    [SerializeField] FodFinDataSet[] fodFinDataSets;
    [SerializeField] int defaultDataSetIndex = 4;
    FodFinDataSet finData;
    [Space(10)]

    // Input components
    [SerializeField] TMP_Dropdown input_taxYear;
    [SerializeField] TMP_InputField input_children;
    [SerializeField] Toggle toggle_nonWorkingSpouse;
    [SerializeField] Toggle toggle_accountSocialInsurance;
    [SerializeField] Toggle toggle_addChildBenefitsToNetIncome;
    [SerializeField] TMP_InputField input_earnedEuroPerHour;
    [SerializeField] TMP_InputField input_workedHoursPerDay;
    [SerializeField] TMP_InputField input_workedDaysPerWeek;
    [SerializeField] TMP_InputField input_undeclaredHoursPerMonth;
    [SerializeField] TextMeshProUGUI deductFactVal;
    [SerializeField] Slider input_deductFactSlider;
    [Range(0f, 1f)] [SerializeField] double DeductiblesFactor = 0.1f;

    int Children;
    bool NonWorkingSpouse = true;
    bool AccountForSocialInsurance = true;
    bool AddChildBenefitsToNetIncome = false;    
    double EuroPerHour = 20f;
    int WorkedHoursPerDay = 8;
    int DaysWorkedPerWeek = 5;
    int MonthlyHoursYouForgotToDeclare = 4;

    // Data handling
    double grossRealEarningsPerMonth;
    double grossRealEarningsPerYear;
    double blackEarningsPerMonth;
    double blackEarningsPerYear;
    double netEarningsPerYear;
    double taxFreeAllowance;
    double taxableIncome;
    double taxBurdenYearly;
    double socialBurdenYearly;
    double childBenefitsYearly;

    // Outpout components
    [SerializeField] TextMeshProUGUI GrossEearningsPerMonth;
    [SerializeField] TextMeshProUGUI GrossEearningsPerYear;
    [SerializeField] TextMeshProUGUI NetEarningsPerMonth;
    [SerializeField] TextMeshProUGUI NetEearningsPerYear;
    [SerializeField] TextMeshProUGUI TaxableIncome;
    [SerializeField] TextMeshProUGUI DeductedExpenses;
    [Space]
    [SerializeField] TextMeshProUGUI SozialBurdenYearly;
    [SerializeField] TextMeshProUGUI SozialBurdenMonthly;
    [SerializeField] TextMeshProUGUI TaxFreeAllowance;
    [SerializeField] TextMeshProUGUI TaxBurdenYearly;
    [SerializeField] TextMeshProUGUI TaxBurdenMonthly;

    // Visualisation components
    const float VISBAR_MAXWIDTH = 475;
    [SerializeField] RectTransform bar_un;
    [SerializeField] RectTransform bar_25;
    [SerializeField] RectTransform bar_40;
    [SerializeField] RectTransform bar_45;
    [SerializeField] RectTransform bar_50;

    // Visualisation data
    double amountTaxedAt25;
    double amountTaxedAt40;
    double amountTaxedAt45;
    double amountTaxedAt50;

    void Start()
    {
        input_children.text = 1.ToString();
        input_earnedEuroPerHour.text = 20.ToString();
        input_undeclaredHoursPerMonth.text = 0.ToString();
        input_workedDaysPerWeek.text = 5.ToString();
        input_workedHoursPerDay.text = 8.ToString();

        toggle_addChildBenefitsToNetIncome.isOn = AddChildBenefitsToNetIncome;
        toggle_accountSocialInsurance.isOn = AccountForSocialInsurance;
        toggle_nonWorkingSpouse.isOn = NonWorkingSpouse;

        input_deductFactSlider.value = 0.1f;

        Application.targetFrameRate = 60;        
        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.SetResolution(438, 618, false);

        AssembleTaxYearData();
    }

    void AssembleTaxYearData()
    {
        System.Collections.Generic.List<string> newOptions = new System.Collections.Generic.List<string>();
        
        for(int i = 0; i<fodFinDataSets.Length;i++)
            newOptions.Add(fodFinDataSets[i].label);

        input_taxYear.ClearOptions();
        input_taxYear.AddOptions(newOptions);

        if(input_taxYear.options.Count <= defaultDataSetIndex)
            input_taxYear.value = defaultDataSetIndex;
        else
            input_taxYear.value = input_taxYear.options.Count;
    }

//--// Main Loop
    void Update()
    {
        ReadInput();
        Process();
        PrintOutPut();
        Visualise();
    }

//--// Stores variable values based on user input
    void ReadInput()
    {
        finData = null;
        for (int i = 0; i < input_taxYear.options.Count; i++)
        {
            if (input_taxYear.options[input_taxYear.value].text == fodFinDataSets[i].label)
            {
                finData = fodFinDataSets[i];
                break;
            }
        }

        if (input_children.text == null || input_children.text == "")
            input_children.text = "0";
        if (int.TryParse(input_children.text, out int count))
            Children = count;

        NonWorkingSpouse = toggle_nonWorkingSpouse.isOn;
        AccountForSocialInsurance = toggle_accountSocialInsurance.isOn;
        AddChildBenefitsToNetIncome = toggle_addChildBenefitsToNetIncome.isOn;

        if (input_earnedEuroPerHour.text == null || input_earnedEuroPerHour.text == "")
            input_earnedEuroPerHour.text = "0";
        if (double.TryParse(input_earnedEuroPerHour.text, out double val0))
            EuroPerHour = val0;

        if (input_workedHoursPerDay.text == null || input_workedHoursPerDay.text == "")
            input_workedHoursPerDay.text = "0";
        if (int.TryParse(input_workedHoursPerDay.text, out int val))
            WorkedHoursPerDay = val;

        if (input_workedDaysPerWeek.text == null || input_workedDaysPerWeek.text == "")
            input_workedDaysPerWeek.text = "0";
        if (int.TryParse(input_workedDaysPerWeek.text, out int val1))
            DaysWorkedPerWeek = val1;

        if (input_undeclaredHoursPerMonth.text == null || input_undeclaredHoursPerMonth.text == "")
            input_undeclaredHoursPerMonth.text = "0";
        if (int.TryParse(input_undeclaredHoursPerMonth.text, out int val2))
            MonthlyHoursYouForgotToDeclare = val2;

        if (MonthlyHoursYouForgotToDeclare < 0) MonthlyHoursYouForgotToDeclare = 0;

        int workedHoursPerMonth = WorkedHoursPerDay * DaysWorkedPerWeek * 4;
        if(val2 > workedHoursPerMonth)
        {
            MonthlyHoursYouForgotToDeclare = workedHoursPerMonth;
            input_undeclaredHoursPerMonth.text = workedHoursPerMonth.ToString("F0");
        }

        deductFactVal.text = (input_deductFactSlider.value * 100f).ToString("F0") + "%";
        DeductiblesFactor = input_deductFactSlider.value;
    }

//--// Processes input values and stores results
    void Process()
    {
        grossRealEarningsPerMonth = EuroPerHour * WorkedHoursPerDay * DaysWorkedPerWeek * 4d;
        grossRealEarningsPerYear = grossRealEarningsPerMonth * 12d;
        blackEarningsPerMonth = EuroPerHour * MonthlyHoursYouForgotToDeclare;
        blackEarningsPerYear = blackEarningsPerMonth * 12d;

        taxFreeAllowance = GetTaxFreeAllowance();
        taxableIncome = GetTaxableIncome(grossRealEarningsPerYear, taxFreeAllowance, DeductiblesFactor, blackEarningsPerYear);
        socialBurdenYearly = GetSocialBurdenYearly(grossRealEarningsPerYear, DeductiblesFactor, blackEarningsPerYear);
        taxBurdenYearly = GetTaxBurdenYearly(taxableIncome);

        if (!AccountForSocialInsurance)
            netEarningsPerYear = grossRealEarningsPerYear - taxBurdenYearly;
        else
            netEarningsPerYear = grossRealEarningsPerYear - taxBurdenYearly - socialBurdenYearly;

        if (AddChildBenefitsToNetIncome)
        {
            childBenefitsYearly = GetChildBenefitsYearly(Children, netEarningsPerYear);
            netEarningsPerYear += childBenefitsYearly;
        }

        if (netEarningsPerYear < 0)
            netEarningsPerYear = 0;
    }

//--// Prints processed data, updates string component
    void PrintOutPut()
    {
        if (!AccountForSocialInsurance)
        {
            SozialBurdenYearly.text = Stringify(0d);
            SozialBurdenMonthly.text = Stringify(0d);
        }   
        else
        {
            SozialBurdenYearly.text = Stringify(socialBurdenYearly);
            SozialBurdenMonthly.text = Stringify(socialBurdenYearly / 12d);
        }

        NetEearningsPerYear.text = Stringify(netEarningsPerYear);
        NetEarningsPerMonth.text = Stringify(netEarningsPerYear / 12d);
        GrossEearningsPerMonth.text = Stringify(grossRealEarningsPerMonth);
        GrossEearningsPerYear.text = Stringify(grossRealEarningsPerMonth * 12);
        TaxFreeAllowance.text = Stringify(taxFreeAllowance);
        TaxableIncome.text = Stringify(taxableIncome);
        TaxBurdenYearly.text = Stringify(taxBurdenYearly);
        TaxBurdenMonthly.text = Stringify(taxBurdenYearly / 12d);
        DeductedExpenses.text = Stringify(grossRealEarningsPerMonth * 12d * input_deductFactSlider.value);
    }

//--// Visualises relative taxation bracket distribution (normalise by sum of brackets and extend sprites rectangularly by respective amount successively)
    void Visualise()
    {
        var sum = taxFreeAllowance + amountTaxedAt25 + amountTaxedAt40 + amountTaxedAt45 + amountTaxedAt50;

        if (sum <= 0)   //  If sum is zero, return to avoid division by zero
        {
            ClearVisualisationBars();
            return;
        }

        var tempUN = taxFreeAllowance / sum;
        var temp25 = amountTaxedAt25 / sum;
        var temp40 = amountTaxedAt40 / sum;
        var temp45 = amountTaxedAt45 / sum;
        var temp50 = amountTaxedAt50 / sum;

        ClearVisualisationBars();
        SetVisualisationBar(bar_un, tempUN);
        SetVisualisationBar(bar_25, tempUN + temp25);
        SetVisualisationBar(bar_40, tempUN + temp25 + temp40);
        SetVisualisationBar(bar_45, tempUN + temp25 + temp40 + temp45);
        SetVisualisationBar(bar_50, tempUN + temp25 + temp40 + temp45 + temp50);
    }

    void SetVisualisationBar(RectTransform rt, double percentage)
    {
        Vector2 size = rt.sizeDelta;
        size.x = VISBAR_MAXWIDTH * (float)percentage;
        rt.sizeDelta = size;
    }
    void ClearVisualisationBars()
    {
        bar_un.sizeDelta = new Vector2(0f, 10f);
        bar_25.sizeDelta = new Vector2(0f, 10f);
        bar_40.sizeDelta = new Vector2(0f, 10f);
        bar_45.sizeDelta = new Vector2(0f, 10f);
        bar_50.sizeDelta = new Vector2(0f, 10f);
    }

    //String formatting and handling
    string Stringify(double input)
    {
        if (input == 0)
            return "€ " + "0";
        else if (input > 10000d)
            return "€ " + input.ToString("F0").Insert(2, ".");
        else if (input > 1000d)
            return "€ " + input.ToString("F0").Insert(1, ".");
        else
            return "€ " + input.ToString("F0");
    }

//--//Returns total social insurance burden for tax year
    double GetSocialBurdenYearly(double grossRealIncome, double deductiblesFactor, double blackEarnings)
    {
        double incomeBasis = grossRealIncome - (grossRealIncome * deductiblesFactor);
        incomeBasis -= blackEarnings;

        double burden = incomeBasis * 0.205d;

        //Cap floor of value based on yearly minimum contributions data
        if (burden < finData.sozialVerzekering_MinQuarterlyContribution * 4)
            burden = finData.sozialVerzekering_MinQuarterlyContribution * 4;

        //Add admin fee using a value of 3.5%
        burden *= 1.035d;

        return burden;
    }

//--//Returns total taxburden for tax year, absolute value
    double GetTaxBurdenYearly(double taxableIncome)
    {
        double taxBurden = 0;

        amountTaxedAt25 = 0;
        amountTaxedAt40 = 0;
        amountTaxedAt45 = 0;
        amountTaxedAt50 = 0;

        double v1 = finData.bracketStart_40Percent;
        double v2 = finData.bracketStart_45Percent;
        double v3 = finData.bracketStart_50Percent;

        // Process income at 25% tax bracket
        if (taxableIncome <= v1)
        {
            amountTaxedAt25 = taxableIncome;
            if (amountTaxedAt25 < 0)
                amountTaxedAt25 = 0;

            amountTaxedAt40 = 0;
            amountTaxedAt45 = 0;
            amountTaxedAt50 = 0;

            return taxableIncome * 0.25d;
        }   
        else
        {
            taxBurden += v1 * 0.25d;

            amountTaxedAt25 = v1;
        }

        // Process income at 40% tax bracket
        if (taxableIncome <= v2)
        {
            amountTaxedAt40 = taxableIncome - v1;
            if (amountTaxedAt40 < 0)
                amountTaxedAt40 = 0;

            amountTaxedAt45 = 0;
            amountTaxedAt50 = 0;

            return taxBurden += (taxableIncome - v1) * 0.4d;
        }
        else
        {
            taxBurden += (v2 - v1) * 0.4d;
            amountTaxedAt40 = v2 - v1;
        }

        // Process income at 45% tax bracket
        if (taxableIncome <= v3)
        {
            amountTaxedAt45 = taxableIncome - v2;
            if (amountTaxedAt45 < 0)
                amountTaxedAt45 = 0;

            amountTaxedAt50 = 0;

            return taxBurden += (taxableIncome - v2) * 0.45d;
        }   
        else
        {
            taxBurden += (v3 - v2) * 0.45d;
            amountTaxedAt45 = v3 - v2;
        }

        // Process income at 50% (maximum) tax bracket        
        amountTaxedAt50 = taxableIncome - v3;
        if (amountTaxedAt50 < 0)
            amountTaxedAt50 = 0;

        taxBurden += (taxableIncome - v3) * 0.5d;

        if (taxBurden < 0)
            taxBurden = 0;

        return taxBurden;
    }

//--// Returns total taxable income, absolute value
    double GetTaxableIncome(double incomeGross, double taxFreeAllowance, double deductibles, double blackEarnings)
    {
        double val = incomeGross - taxFreeAllowance;
        val -= incomeGross * deductibles;
        val -= blackEarnings;

        if (val < 0)
            val = 0;

        return val;
    }


//--// Returns the applicable tax free allowance, absolute value
    double GetTaxFreeAllowance()
    {
        double ceiling;
        if (Children == 0)
            ceiling = finData.taxFreeAllowance;
        else if (Children == 1)
            ceiling = finData.taxFreeAllowance + finData.allowanceIncreaseChildCount_1;
        else if (Children == 2)
            ceiling = finData.taxFreeAllowance + finData.allowanceIncreaseChildCount_2;
        else if (Children == 3)
            ceiling = finData.taxFreeAllowance + finData.allowanceIncreaseChildCount_3;
        else if (Children == 4)
            ceiling = finData.taxFreeAllowance + finData.allowanceIncreaseChildCount_4;
        else
            ceiling = finData.taxFreeAllowance + finData.allowanceIncreaseChildCount_4 + (finData.allowanceIncreasePerChildAboveCountFour * (Children - 4));

        if (NonWorkingSpouse)
            ceiling += finData.allowanceIncreaseNonWorkingSpouse;

        if (ceiling < 0)
            ceiling = 0;

        return ceiling;
    }

    double GetChildBenefitsYearly(int childCount, double netTaxableIncomeYearly)
    {
        double amount = childCount * 180d;

        if(Children <= 2)
        {
            if(netTaxableIncomeYearly <= 40187d)
                amount += childCount * 72d;
            if (netTaxableIncomeYearly > 40187d && netTaxableIncomeYearly <= 46885d)
                amount += childCount * 36d;
        }
        else
        {
            if (netTaxableIncomeYearly <= 40187d)
                amount += childCount * 106d;
            if (netTaxableIncomeYearly > 40187d && netTaxableIncomeYearly <= 75593d)
                amount += childCount * 83d;
        }

        return amount * 12;
    }
}
