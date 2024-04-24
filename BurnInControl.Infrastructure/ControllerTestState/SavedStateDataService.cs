using BurnInControl.Data.BurnInTests;
using BurnInControl.Shared.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ErrorOr;

namespace BurnInControl.Infrastructure.ControllerTestState;

public class SavedStateDataService {
    private readonly IMongoCollection<SavedStateLog> _savedStateLogCollection;

    public SavedStateDataService(IMongoClient client,IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.DatabaseName ?? "burn_in_db");
        this._savedStateLogCollection = database.GetCollection<SavedStateLog>("saved_states");
    }

    public async Task<ErrorOr<Success>> UpdateLog(ObjectId logId,ControllerSavedState savedState) {
        var update=Builders<SavedStateLog>.Update.Set(e=>e.SavedState,savedState).Set(e=>e.TimeStamp,DateTime.Now);
        var result=await this._savedStateLogCollection.UpdateOneAsync(e=>e._id==logId,update);
        return result.IsAcknowledged ? Result.Success : Error.Failure(description:$"Failed to update log {logId}");
    }
    
    public async Task<ErrorOr<SavedStateLog>> SaveState(ControllerSavedState savedState,ObjectId logId,string stationId) {
        var log=new SavedStateLog() {
            _id = ObjectId.GenerateNewId(),
            TimeStamp=DateTime.Now,
            TestId=savedState.TestId,
            StationId=stationId,
            LogId=logId,
            SavedState=savedState
        };
        await this._savedStateLogCollection.InsertOneAsync(log);
        var result=await this._savedStateLogCollection.Find(e=>e._id==log._id).FirstOrDefaultAsync();
        return result != null ? result : Error.Failure(description:"Failed to save state");
    }

    public async Task<ErrorOr<SavedStateLog>> GetSavedState(string? testId=default,ObjectId? logId=default,ObjectId? id=default) {
        if (!string.IsNullOrEmpty(testId)) {
            var logResult = await this._savedStateLogCollection.Find(e => e.SavedState.TestId == testId).FirstOrDefaultAsync();
            
            return logResult != null ? logResult : Error.NotFound(description:$"TestId {testId} not found");
        }
        
        if (logId != null) {
            var logResult = await this._savedStateLogCollection.Find(e => e.LogId == logId).FirstOrDefaultAsync();
            return logResult != null ? logResult : Error.NotFound(description:$"LogId {logId} not found");
        }
        
        if (id != null) {
            var logResult = await this._savedStateLogCollection.Find(e => e._id == id).FirstOrDefaultAsync();
            return logResult != null ? logResult : Error.NotFound(description:$"_id {id} not found");
        }
        
        return Error.Unexpected(description: "TestId, LogId, and Id were null");
    }

    public Task<List<SavedStateLog>?> GetSavedStates(string? stationId=default) {
        if (string.IsNullOrEmpty(stationId)) {
            return this._savedStateLogCollection.Find(_ => true).ToListAsync();
        }
        return this._savedStateLogCollection.Find(e => e.StationId == stationId).ToListAsync();
    } 

    public async Task<ErrorOr<Success>> ClearSavedState(string? testId=default,ObjectId? logId=default,ObjectId? id=default) {
        if (!string.IsNullOrEmpty(testId)) {
            var deleteResult=await this._savedStateLogCollection.DeleteOneAsync(e => e.SavedState.TestId == testId);
            
            return deleteResult.IsAcknowledged ? Result.Success : Error.Failure(description:$"TestId {testId} failed to delete");
        }
        
        if (logId != null) {
            var deleteResult=await this._savedStateLogCollection.DeleteOneAsync(e => e.LogId == logId);
            return deleteResult.IsAcknowledged ? Result.Success : Error.Failure(description:$"LogId {logId} failed to delete");
        }
        
        if (id != null) {
            var deleteResult=await this._savedStateLogCollection.DeleteOneAsync(e => e._id == id);
            return deleteResult.IsAcknowledged ? Result.Success : Error.Failure(description:$"_id {id} failed to delete");
        }
        
        return Error.Unexpected(description: "TestId, LogId, and Id were null");
    }
}