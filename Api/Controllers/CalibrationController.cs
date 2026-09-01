using Infrastructure.Data;
using Management.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/calibration")]
public class CalibrationController
    : ControllerBase
{
    private readonly SportsBettingDbContext
        _db;

    public CalibrationController(
        SportsBettingDbContext db)
    {
        _db = db;
    }

    // =====================================
    // CALIBRATION DATA
    // =====================================

    [HttpGet]
    public async Task<
        ActionResult<
            List<ConfidenceCalibrationDto>>>
                GetAsync()
    {
        var data =
            await _db
                .ConfidenceCalibrations
                .OrderBy(x =>
                    x.ConfidenceBand)
                .Select(x =>
                    new ConfidenceCalibrationDto
                    {
                        ConfidenceBand =
                            x.ConfidenceBand,

                        TotalPredictions =
                            x.TotalPredictions,

                        CorrectPredictions =
                            x.CorrectPredictions,

                        ActualAccuracy =
                            x.ActualAccuracy,

                        CalibrationAdjustment =
                            x.CalibrationAdjustment,

                        LastUpdatedUtc =
                            x.LastUpdatedUtc,

                        OverConfident =
                            x.CalibrationAdjustment
                                < -5,

                        UnderConfident =
                            x.CalibrationAdjustment
                                > 5
                    })
                .ToListAsync();

        return Ok(data);
    }
}
