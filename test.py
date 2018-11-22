import time
import threading
import msvcrt
import pygame
from mutagen.mp3 import MP3 as mp3

flag = True
filename = 'C:\\Users\\Tsurusaki\\Documents\\hayaoshi\\buttonSound.mp3' 
pygame.mixer.init() 
pygame.mixer.music.load(filename)
mp3Length = mp3(filename).info.length

def button():
  global flag
  while True:
    if msvcrt.kbhit():
      kb = msvcrt.getch()
      if kb.decode() in {'f', 'j'}:
        print(kb.decode() + "が押されました")
        pygame.mixer.music.play(1)
        time.sleep(mp3Length + 0.25)
        pygame.mixer.music.stop()
        flag = False
        break
    time.sleep(1/60)

th = threading.Thread(target=button, daemon=True)
th.start()

i = 0
print("start")
while True:
  time.sleep(1/60)
  if not flag:
    print(i)
    break
  i += 1

print("終わります")