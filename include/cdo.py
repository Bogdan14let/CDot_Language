import os

def cdoParse(file_name: str, list_name: list):
    # --- Проверка расширения файла ---
    # spt 0s "." exts -> разбиваем имя файла по точкам
    exts = file_name.split(".")
    
    # len exts 2f / dec 2f 2f / get exts 2f 3s -> берем последнее расширение
    if exts[-1] != "cdo":
        # Имитируем твою команду: mov 5m 0 1 "Invalid file extension..."
        # В Python это можно обработать через print или вызвать исключение (Exception)
        print(f"Error: Invalid file extension, must be '.cdo'!")
        return

    # --- Чтение файла и разбиение на строки ---
    # rdl 0s 2s -> читаем весь файл
    # spt 2s "\n" lines -> бьем по строкам
    if not os.path.exists(file_name):
        return
        
    with open(file_name, "r", encoding="utf-8") as f:
        lines = f.read().split("\n")

    # Инициализация переменных (твои mov 3f 0, mov 6s "" и т.д.)
    method = ""
    info = ""
    from_var = ""
    
    start_index = -1

    # --- Цикл cdo.ParseStart: Ищем строку "start" ---
    for i in range(len(lines)):
        line_clean = lines[i].strip().lower() # trm и low
        if line_clean == "start":
            start_index = i
            break

    # Если "start" не нашли, выходим
    if start_index == -1:
        return

    # --- Цикл cdo.ParseGetSource: Читаем команды после "start" ---
    # Перебираем строки, начиная со следующей после "start"
    for i in range(start_index + 1, len(lines)):
        line_clean = lines[i].strip() # trm 0s 0s
        if not line_clean:
            continue

        # spt 0s ":" command
        command_parts = line_clean.split(":")
        cmd_type = command_parts[0].strip().lower() # get command 0 0s + low

        if cmd_type == "end":
            break # jf 0s == "end" cdo.ParseEnd

        # Проверяем, что есть правая часть после двоеточия (значение команды)
        if len(command_parts) < 2:
            continue

        cmd_value = command_parts[1].strip() # get command 1 0s + trm

        # Обработка конкретных команд
        if cmd_type == "method":
            method = cmd_value # mov 6s 0s
            
        elif cmd_type == "info":
            # spt 0s "," words / len words 7f (в твоем коде words дальше не используется, но мы сохраняем логику)
            words = cmd_value.split(",") 
            info = cmd_value # mov 7s 0s
            
        elif cmd_type == "from":
            from_var = cmd_value # mov 8s 0s

    # --- Блок cdo.ParseEnd: Сохранение результата ---
    # push 1s {method=6s, info=7s, from=8s}
    result_data = {
        "method": method,
        "info": info,
        "from": from_var
    }
    list_name.append(result_data)

    # Очистка регистров (mov 0s "" и т.д.) в Python происходит автоматически 
    # при выходе из функции, так как все переменные локальные.
    return