using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Taxomatico : MonoBehaviour
{
    // Inpout components
    [SerializeField] TMP_InputField input_children;
    [SerializeField] Toggle toggle_nonWorkingSpouse;
    [SerializeField] Toggle toggle_accountSocialInsurance;
    [SerializeField] TMP_InputField input_earnedEuroPerHour;
    [SerializeField] TMP_InputField input_workedHoursPerDay;
    [SerializeField] TMP_InputField input_workedDaysPerWeek;
    [SerializeField] TMP_InputField input_undeclaredHoursPerMonth;
    [SerializeField] TextMeshProUGUI deductFactVal;
    [SerializeField] Slider deductFactSlider;
    [Range(0f, 1f)] [SerializeField] double DeductiblesFactor = 0.1f;

    int Children;
    bool NonWorkingSpouse = true;
    bool AccountForSocialInsurance = true;
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

        toggle_accountSocialInsurance.isOn = AccountForSocialInsurance;
        toggle_nonWorkingSpouse.isOn = NonWorkingSpouse;
        //toggle_childBenefits.isOn = ChildBenefits;

        deductFactSlider.value = 0.1f;

        Application.targetFrameRate = 60;        
        Screen.fullScreenMode = FullScreenMode.Windowed;        
    }

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
        if (int.TryParse(input_children.text, out int count))
            Children = count;

        NonWorkingSpouse = toggle_nonWorkingSpouse.isOn;
        AccountForSocialInsurance = toggle_accountSocialInsurance.isOn;
        //ChildBenefits = toggle_childBenefits.isOn;

        if (double.TryParse(input_earnedEuroPerHour.text, out double val0))
            EuroPerHour = val0;

        if (int.TryParse(input_workedHoursPerDay.text, out int val))
            WorkedHoursPerDay = val;

        if (int.TryParse(input_workedDaysPerWeek.text, out int val1))
            DaysWorkedPerWeek = val1;

        if (int.TryParse(input_undeclaredHoursPerMonth.text, out int val2))
            MonthlyHoursYouForgotToDeclare = val2;

        if (MonthlyHoursYouForgotToDeclare < 0)
            MonthlyHoursYouForgotToDeclare = 0;

        int workedHoursPerMonth = WorkedHoursPerDay * DaysWorkedPerWeek * 4;
        if(val2 > workedHoursPerMonth)
        {
            MonthlyHoursYouForgotToDeclare = workedHoursPerMonth;
            input_undeclaredHoursPerMonth.text = workedHoursPerMonth.ToString("F0");
        }

        deductFactVal.text = (deductFactSlider.value * 100f).ToString("F0") + "%";
        DeductiblesFactor = deductFactSlider.value;
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
        DeductedExpenses.text = Stringify(grossRealEarningsPerMonth * 12d * deductFactSlider.value);
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

        if (burden < 3460d)
            burden = 3460d;

        return burden * 1.03d;
    }

//--//Returns total taxburden for tax year, absolute value
    double GetTaxBurdenYearly(double taxableIncome)
    {
        double taxBurden = 0;

        amountTaxedAt25 = 0;
        amountTaxedAt40 = 0;
        amountTaxedAt45 = 0;
        amountTaxedAt50 = 0;

        // Process income at 25% tax bracket
        if (taxableIncome <= 15820d)
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
            taxBurden += 15820d * 0.25d;

            amountTaxedAt25 = 15820d;
        }

        // Process income at 40% tax bracket
        if (taxableIncome <= 27920d)
        {
            amountTaxedAt40 = taxableIncome - 15820d;
            if (amountTaxedAt40 < 0)
                amountTaxedAt40 = 0;

            amountTaxedAt45 = 0;
            amountTaxedAt50 = 0;

            return taxBurden += (taxableIncome - 15820d) * 0.4d;
        }
        else
        {
            taxBurden += (27920d - 15820d) * 0.4d;
            amountTaxedAt40 = 27920d - 15820d;
        }

        // Process income at 45% tax bracket
        if (taxableIncome <= 48320d)
        {
            amountTaxedAt45 = taxableIncome - 27920d;
            if (amountTaxedAt45 < 0)
                amountTaxedAt45 = 0;

            amountTaxedAt50 = 0;

            return taxBurden += (taxableIncome - 27920d) * 0.45d;
        }   
        else
        {
            taxBurden += (48320d - 27920d) * 0.45d;
            amountTaxedAt45 = 48320d - 27920d;
        }

        // Process income at 50% (maximum) tax bracket        
        amountTaxedAt50 = taxableIncome - 48320d;
        if (amountTaxedAt50 < 0)
            amountTaxedAt50 = 0;

        taxBurden += (taxableIncome - 48320d) * 0.5d;

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

    // Hardcoded values, should expose as parameters to allow user to select tax year?
    // Returns the applicable tax free allowance, absolute value
    double GetTaxFreeAllowance()
    {
        double ceiling;
        if (Children == 0)
            ceiling = 10570d;
        else if (Children == 1)
            ceiling = 10570d + 1920d;
        else if (Children == 2)
            ceiling = 10570d + 4410d;
        else if (Children == 3)
            ceiling = 10570d + 11090d;
        else if (Children == 4)
            ceiling = 10570d + 17940d;
        else
            ceiling = 10570d + 17940d + (6850d * (Children - 4));

        if (NonWorkingSpouse)
            ceiling += 13050d;

        if (ceiling < 0)
            ceiling = 0;

        return ceiling;
    }
}
