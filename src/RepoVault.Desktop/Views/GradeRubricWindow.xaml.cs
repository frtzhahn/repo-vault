using System;
using System.ComponentModel;
using System.Windows;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class GradeRubricWindow : Window
{
    private readonly AppSession _session;
    private readonly RepositoryRecord _repo;
    private readonly string _courseCode;
    private readonly string _ownerDisplay;

    public GradeRubricWindow() : this(new AppSession(), new RepositoryRecord(), "CS201", "Student")
    {
    }

    public GradeRubricWindow(
        AppSession session, RepositoryRecord repo, string courseCode, string ownerDisplay)
    {
        _session = session;
        _repo = repo;
        _courseCode = courseCode;
        _ownerDisplay = ownerDisplay;
        InitializeComponent();

        LblRepoHeader.Text = $"{_repo.Title} ({_repo.RepoSlug})";
        LblCourseAndOwner.Text = $"Course: {_courseCode}  |  Owner: {_ownerDisplay}";
        LblMetricsSummary.Text =
            $"Code Metrics: {_repo.SourceLines:N0} Source LOC  |  {_repo.CommentLines:N0} Comment LOC  |  {_repo.BlankLines:N0} Blank LOC  |  {_repo.TotalFiles} Files";
        Title = $"Grade Rubric - {_repo.RepoSlug}";
        UpdateScoreLabels();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this) || _repo.RepoId == 0)
        {
            return;
        }

        try
        {
            EvaluationRecord? eval = await _session.Evaluations.GetEvaluationByRepoAsync(_repo.RepoId);
            if (eval is not null)
            {
                SldArch.Value = Math.Clamp(eval.ArchitectureScore, 0, 25);
                SldFunc.Value = Math.Clamp(eval.FunctionalityScore, 0, 25);
                SldQual.Value = Math.Clamp(eval.CodeQualityScore, 0, 25);
                SldDoc.Value = Math.Clamp(eval.DocumentationScore, 0, 25);
                TxtCritiqueNotes.Text = eval.CritiqueNotes ?? string.Empty;
                UpdateScoreLabels();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not load existing evaluation: " + ex.Message,
                "Grade Rubric", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateScoreLabels();
    }

    private void UpdateScoreLabels()
    {
        if (LblTotalScore is null)
        {
            return;
        }

        int arch = (int)Math.Round(SldArch.Value);
        int func = (int)Math.Round(SldFunc.Value);
        int qual = (int)Math.Round(SldQual.Value);
        int doc = (int)Math.Round(SldDoc.Value);
        int total = arch + func + qual + doc;

        LblValArch.Text = $"{arch} / 25";
        LblValFunc.Text = $"{func} / 25";
        LblValQual.Text = $"{qual} / 25";
        LblValDoc.Text = $"{doc} / 25";
        LblTotalScore.Text = $"Total: {total} / 100";
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void BtnSaveRubric_Click(object sender, RoutedEventArgs e)
    {
        User? actor = _session.GetCurrentUserEntity();
        if (actor is null)
        {
            return;
        }

        int arch = (int)Math.Round(SldArch.Value);
        int func = (int)Math.Round(SldFunc.Value);
        int qual = (int)Math.Round(SldQual.Value);
        int doc = (int)Math.Round(SldDoc.Value);
        string notes = TxtCritiqueNotes.Text.Trim();
        bool lockRepo = ChkLockAfterSave.IsChecked == true;

        BtnSaveRubric.IsEnabled = false;
        try
        {
            var (ok, message, _) = await _session.Evaluations.SaveEvaluationAsync(
                actor, _repo.RepoId, arch, func, qual, doc, notes, lockRepo);

            if (!ok)
            {
                MessageBox.Show(this, message, "Grade Rubric", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not save evaluation: " + ex.Message,
                "Grade Rubric", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnSaveRubric.IsEnabled = true;
        }
    }
}
