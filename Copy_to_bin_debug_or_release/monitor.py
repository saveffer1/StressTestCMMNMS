from matplotlib import pyplot as plt
from matplotlib.animation import FuncAnimation
import psutil
from itertools import count
import time
from tkinter import PhotoImage

def plot():
    fig, (ax1, ax2) = plt.subplots(2)
    y1 = []
    y2 = []
    x = []
    fig.canvas.manager.set_window_title('mmnms Stress Test & CPU/RAM Usage')
    # plt.Figure()
    # thismanager = plt.get_current_fig_manager()
    # img = PhotoImage('tcddr.ppm')
    # thismanager.window.tk.call('wm', 'iconphoto', thismanager.window._w, img)
    
    def animate(i):
        t = time.time()
        x.append(t)
        y1.append(psutil.cpu_percent())
        y2.append((psutil.virtual_memory().used >> 20)/1000)
        ax1.cla()
        ax2.cla()
        ax1.grid()
        ax2.grid()
        ax1.set_title("CPU Usage")
        ax2.set_title("Memory Usage")
        fig.subplots_adjust(hspace=0.4)
        ax1.set(ylabel="% Usage")
        ax2.set(ylabel="Memory in GB", xlabel="time")
        ax1.plot(x,y1)
        ax2.plot(x,y2)
        ax1.annotate('%0.2f' % y1[-1], xy=(1, y1[-1]), xytext=(8, 0),
                 xycoords=('axes fraction', 'data'), textcoords='offset points')
        ax2.annotate('%0.2f' % y2[-1], xy=(1, y2[-1]), xytext=(8, 0),
                 xycoords=('axes fraction', 'data'), textcoords='offset points')
    ani = FuncAnimation(fig, animate, interval=(1000))
    plt.show()

