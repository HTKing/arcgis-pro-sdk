/*

   Copyright 2018 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace SDKExamples.GeodatabaseSDK
{
  /// <summary>
  /// Illustrates how to find fields from a Row or Feature.
  /// </summary>
  /// 
  /// <remarks>
  /// <para>
  /// While it is true classes that are derived from the <see cref="ArcGIS.Core.CoreObjectsBase"/> super class 
  /// consumes native resources (e.g., <see cref="ArcGIS.Core.Data.Geodatabase"/> or <see cref="ArcGIS.Core.Data.FeatureClass"/>), 
  /// you can rest assured that the garbage collector will properly dispose of the unmanaged resources during 
  /// finalization.  However, there are certain workflows that require a <b>deterministic</b> finalization of the 
  /// <see cref="ArcGIS.Core.Data.Geodatabase"/>.  Consider the case of a file geodatabase that needs to be deleted 
  /// on the fly at a particular moment.  Because of the <b>indeterministic</b> nature of garbage collection, we can't
  /// count on the garbage collector to dispose of the Geodatabase object, thereby removing the <b>lock(s)</b> at the  
  /// moment we want. To ensure a deterministic finalization of important native resources such as a 
  /// <see cref="ArcGIS.Core.Data.Geodatabase"/> or <see cref="ArcGIS.Core.Data.FeatureClass"/>, you should declare 
  /// and instantiate said objects in a <b>using</b> statement.  Alternatively, you can achieve the same result by 
  /// putting the object inside a try block and then calling Dispose() in a finally block.
  /// </para>
  /// <para>
  /// In general, you should always call Dispose() on the following types of objects: 
  /// </para>
  /// <para>
  /// - Those that are derived from <see cref="ArcGIS.Core.Data.Datastore"/> (e.g., <see cref="ArcGIS.Core.Data.Geodatabase"/>).
  /// </para>
  /// <para>
  /// - Those that are derived from <see cref="ArcGIS.Core.Data.Dataset"/> (e.g., <see cref="ArcGIS.Core.Data.Table"/>).
  /// </para>
  /// <para>
  /// - <see cref="ArcGIS.Core.Data.RowCursor"/> and <see cref="ArcGIS.Core.Data.RowBuffer"/>.
  /// </para>
  /// <para>
  /// - <see cref="ArcGIS.Core.Data.Row"/> and <see cref="ArcGIS.Core.Data.Feature"/>.
  /// </para>
  /// <para>
  /// - <see cref="ArcGIS.Core.Data.Selection"/>.
  /// </para>
  /// <para>
  /// - <see cref="ArcGIS.Core.Data.VersionManager"/> and <see cref="ArcGIS.Core.Data.Version"/>.
  /// </para>
  /// </remarks>
  public class RowFindField
  {
    /// <summary>
    /// In order to illustrate that Geodatabase calls have to be made on the MCT.
    /// </summary>
    /// <returns></returns>
    public async Task RowFindFieldAsync()
    {
      await QueuedTask.Run(() => MainMethodCode());
    }

    public void MainMethodCode()
    {
      using (Geodatabase fileGeodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\Data\LocalGovernment.gdb"))))
      using (Table table                 = fileGeodatabase.OpenDataset<Table>("luCodeInspection"))
      {
        QueryFilter queryFilter = new QueryFilter { WhereClause = "ACTION = '1st Notice'" };

        using (RowCursor cursor = table.Search(queryFilter)) // Using Recycling.
        {
          bool hasRow = cursor.MoveNext();
            
          using (Row currentRow = cursor.Current)
          {
            TableDefinition tableDefinition = table.GetDefinition();

            int trivialFindField   = currentRow.FindField(currentRow.GetFields()[0].Name); // This will (obviously) give 0.
            int inspectIDIndex     = currentRow.FindField("INSPECTID");
            int violateKeyIndex    = currentRow.FindField("violatekey");
            int workKeyIndex       = currentRow.FindField("WorkKey");
            int subtypeFieldIndex  = currentRow.FindField(tableDefinition.GetSubtypeField()); // Will be -1 since there are no subtypes.
            int objectIdFieldIndex = currentRow.FindField(tableDefinition.GetObjectIDField());
            int minusOne           = currentRow.FindField("Gibberish"); // This will be -1.
          }
        }
      }

      // Opening a Non-Versioned SQL Server instance.

      DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.SQLServer)
      {
        AuthenticationMode = AuthenticationMode.DBMS,
     
        // Where testMachine is the machine where the instance is running and testInstance is the name of the SqlServer instance.
        Instance = @"testMachine\testInstance",

        // Provided that a database called LocalGovernment has been created on the testInstance and geodatabase has been enabled on the database.
        Database = "LocalGovernment",

        // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
        User     = "gdb",
        Password = "password",
        Version  = "dbo.DEFAULT"
      };

      using (Geodatabase geodatabase              = new Geodatabase(connectionProperties))
      using (FeatureClass landUseCaseFeatureClass = geodatabase.OpenDataset<FeatureClass>("LocalGovernment.GDB.LandUseCase"))
      {
        QueryFilter filter = new QueryFilter { WhereClause = "CASETYPE = 'Rezoning'" };
        
        using (RowCursor landUseCursor = landUseCaseFeatureClass.Search(filter, false))
        {
          FeatureClassDefinition featureClassDefinition = landUseCaseFeatureClass.GetDefinition();

          while (landUseCursor.MoveNext())
          {
            Feature rezoningUseCase = (Feature)landUseCursor.Current;

            int caseIdIndex     = rezoningUseCase.FindField("CASEID");
            int caseNameIndex   = rezoningUseCase.FindField("casename");
            int applicantIndex  = rezoningUseCase.FindField("Applicant");
            int shapeFieldIndex = rezoningUseCase.FindField(featureClassDefinition.GetShapeField());
            int subtypeIndex    = rezoningUseCase.FindField(featureClassDefinition.GetSubtypeField()); // Will be -1 since there are no subtypes.
            int objectIdIndex   = rezoningUseCase.FindField(featureClassDefinition.GetObjectIDField());

            break;
          }
        }
      }
    } 
  }
}