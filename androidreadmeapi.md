# راهنمای پیاده‌سازی اندروید برای TrelloJiraCore

این راهنما برای توسعه‌دهندگان موبایل جهت اتصال اپلیکیشن اندروید به بک‌آند TrelloJiraCore تهیه شده است.

## احراز هویت
سیستم از Bearer Token برای احراز هویت استفاده می‌کند. توکن دریافتی باید در هدر تمام درخواست‌های HTTP قرار گیرد:
`Authorization: Bearer {your_token}`

## اتصال به API
برای دریافت اطلاعات بوردها از کتابخانه‌هایی مانند Retrofit یا Volley استفاده کنید.

### مثال ساختار داده بورد (GET /api/boards/1)
```json
{
  "id": 1,
  "title": "پروژه اندروید",
  "lists": [
    {
      "id": 1,
      "title": "برای انجام",
      "cards": [...]
    }
  ]
}
```

## اتصال آنی (Real-time) با SignalR
برای اندروید از کتابخانه رسمی SignalR Java Client استفاده کنید:
`implementation 'com.microsoft.signalr:signalr:8.0.0'`

### کد نمونه اتصال
```java
HubConnection hubConnection = HubConnectionBuilder.create("http://10.0.2.2:5000/boardHub").build();

hubConnection.on("OnCardMoved", (cardId, fromListId, toListId) -> {
    // بروزرسانی رابط کاربری در ترد اصلی
}, Integer.class, Integer.class, Integer.class);

hubConnection.start();
hubConnection.send("JoinBoard", 1);
```
نکته: در شبیه‌ساز اندروید برای دسترسی به لوکال‌هوست سیستم از IP `10.0.2.2` استفاده کنید.
