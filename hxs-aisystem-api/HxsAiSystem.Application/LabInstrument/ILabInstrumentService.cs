namespace HxsAiSystem.Application.LabInstrument;

public interface ILabInstrumentService
{
    Task<List<InstrumentDto>> GetInstrumentsAsync(bool availableOnly = false);
    Task<InstrumentDto> CreateInstrumentAsync(InstrumentRequest request);
    Task UpdateInstrumentAsync(Guid id, InstrumentRequest request);
    Task<List<BookingDto>> GetBookingsAsync(bool mine = false, string? status = null);
    Task<BookingDto> CreateBookingAsync(BookingRequest request);
    Task CancelBookingAsync(Guid id);
    Task ApproveBookingAsync(Guid id, ApprovalRequest request);
    Task RejectBookingAsync(Guid id, ApprovalRequest request);
    Task CompleteBookingAsync(Guid id);
    Task<List<UsageDto>> GetUsagesAsync(bool mine = false);
    Task<UsageDto> CreateUsageAsync(UsageRequest request);
    Task<List<RepairDto>> GetRepairsAsync(bool mine = false, string? status = null);
    Task<RepairDto> CreateRepairAsync(RepairRequest request);
    Task ApproveRepairAsync(Guid id, ApprovalRequest request);
    Task RejectRepairAsync(Guid id, ApprovalRequest request);
    Task StartRepairAsync(Guid id, RepairWorkRequest request);
    Task CompleteRepairAsync(Guid id, RepairWorkRequest request);
}
