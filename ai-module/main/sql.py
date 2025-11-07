import psycopg2

try:
    conn = psycopg2.connect(
        dbname="database",
        user="postgres",
        password="123",
        host="localhost",
        port="5432"
    )

    cur = conn.cursor()
    cur.execute("SELECT * FROM users;")
    rows = cur.fetchall()

    for row in rows:
        print(row)

    cur.close()
    conn.close()
except Exception as e:
    print("Database error:", e)
