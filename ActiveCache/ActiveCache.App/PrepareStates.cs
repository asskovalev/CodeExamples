using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActiveCache.App
{
	class PrepareStates
	{
		public static List<PrepareState> Get()
		{
			return All
				.Select(it =>
				{
					it.OrderType = "InteractionWithSpa";
					it.PreapareTypeId = 1;
					return it;
				})
				.ToList();

		}

		private static List<PrepareState> All =
			new List<PrepareState>()
			{
				new PrepareState {
Id = 200,
Name = "Связывание",
Code = "Production_Linking",
ThreadCount = 10,
RecordCount = 1,
ExecutionSpan = 1000,
CheckSpan = 300000,
IsThreaded = true,
}, new PrepareState {
Id = 291,
Name = "Ожидание связывания",
Code = "Production_WaitingLinking",
ThreadCount = 1,
RecordCount = 40,
ExecutionSpan = 1000,
CheckSpan = 300000,
IsThreaded = true,
}, new PrepareState {
Id = 199,
Name = "Создание файла производителю",
Code = "Production_CreateProductionFile",
ThreadCount = 3,
RecordCount = 40,
ExecutionSpan = 1000,
CheckSpan = 300000,
IsThreaded = true,
}, new PrepareState {
Id = 202,
Name = "Ожидание оприходования",
Code = "Production_WaitingProductionFile",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 300000,
IsThreaded = false,
}, new PrepareState {
Id = 207,
Name = "Оприходование на складе",
Code = "Production_PostingEquipmentToStock",
ThreadCount = 1,
RecordCount = 50,
ExecutionSpan = 1000,
CheckSpan = 300000,
IsThreaded = true,
}, new PrepareState {
Id = 263,
Name = "Синхронизация оприходования",
Code = "WaitReciveNewComplectProduction",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 300000,
IsThreaded = true,
}, new PrepareState {
Id = 249,
Name = "Ожидание предпродажной подготовки",
Code = "Production_WaitingForPrepareForSale",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 300000,
IsThreaded = false,
}, new PrepareState {
Id = 236,
Name = "Создание в CRM",
Code = "Production_CRMProcess",
ThreadCount = 1,
RecordCount = 40,
ExecutionSpan = 1000,
CheckSpan = 300000,
IsThreaded = true,
}, new PrepareState {
Id = 240,
Name = "Добавление блокировок в CRM",
Code = "Production_AddServiceBlockCRM",
ThreadCount = 2,
RecordCount = 20,
ExecutionSpan = 1000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 232,
Name = "Синхронизация с Collection",
Code = "Production_Synchronize",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 209,
Name = "Изменение места хранения баланса в RI",
Code = "Production_SetBalanceStorageForPaInRI",
ThreadCount = 1,
RecordCount = 40,
ExecutionSpan = 1000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 208,
Name = "Установка статусов в RI тип C",
Code = "Production_ChangeStatusInRITypeC",
ThreadCount = 4,
RecordCount = 40,
ExecutionSpan = 1000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 296,
Name = "Создание абонента на платформах",
Code = "Production_CreateSubscriberOnPlatforms",
ThreadCount = 1,
RecordCount = 10,
ExecutionSpan = 6000,
CheckSpan = 300000,
IsThreaded = true },
new PrepareState {
Id = 296,
 Name = "Создание абонента на платформах",
Code = "Production_CreateSubscriberOnPlatforms",
ThreadCount = 10,
RecordCount = 10,
ExecutionSpan = 6000,
CheckSpan = 300000,
IsThreaded = true,
}, new PrepareState {
Id = 297,
Name = "Ожидание ответа на Создание абонента на платформах",
Code = "Production_WaitCreateSubscriberOnPlatforms",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 300000,
IsThreaded = false,
}, new PrepareState {
Id = 230,
Name = "Перемещение со склада",
Code = "Production_MoveEquipment",
ThreadCount = 1,
RecordCount = 1,
ExecutionSpan = 6000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 247,
Name = "Ожидание отгрузки дилеру",
Code = "Production_WaitingForSell",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 600,
CheckSpan = 30000,
IsThreaded = false,
}, new PrepareState {
Id = 243,
Name = "Изменение пользователя",
Code = "Production_ChangeUserCRMData",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 246,
Name = "Регистрация заявки на разблокировку",
Code = "Production_UnblockRequest",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 244,
Name = "Тарификация",
Code = "Production_SetTariffication",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 242,
Name = "Ожидание тарификации",
Code = "Production_WaitingForTariffication",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 1000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 241,
Name = "Перевод на склад дилера",
Code = "Production_MoveToBayer",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 329,
Name = "Ожидание синхронизации при отгрузке дилеру",
Code = "WaitingSellCompleteMoveEquipment",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 1000,
CheckSpan = 30000,
IsThreaded = true,
}, new PrepareState {
Id = 223,
Name = "Выполнено",
Code = "Production_Ok",
ThreadCount = 1,
RecordCount = 20,
ExecutionSpan = 6000,
CheckSpan = 30000,
IsThreaded = false,
}
			};
	}
}
