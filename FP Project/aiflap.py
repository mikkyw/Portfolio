import neat
import pygame
import time
import os
import random
pygame.font.init()

WIDTH, HEIGHT = 400, 500
BUN_IMAGES = [pygame.image.load('bun1.png'), pygame.image.load('bun2.png'),  pygame.image.load('bun3.png')]
BG_IMAGE = pygame.image.load('space.jpeg')  
PIPE_IMAGE = pygame.image.load('pipe.png')
BASE_IMAGE = pygame.image.load('bottom.png')
STAT_FONT = pygame.font.SysFont("comicsans", 50)
class Bun:
    IMGS = BUN_IMAGES
    ANIMATION_TIME = 2
    ROTATION = 25
    ROTATION_VELOCITY = 20

    def __init__(self,x,y):
       self.x = x
       self.y = y
       self.tilt = 0
       self.tick = 0
       self.velocity = 0
       self.height = self.y
       self.img = self.IMGS[0]
       self.imgCount = 0

    def jump(self):
        self.velocity = -8.5
        self.tick = 0
        self.height = self.y

    def move(self):
        self.tick += 1
        d = self.velocity*self.tick  + 1.5*self.tick**2 #arc of player
        
        if d >= 16: #capping off jump
            d = 16
        
        if d < 0: #jump
            d -= 2

        self.y = self.y + d

        if d < 0 or self.y < self.height + 50:
            if self.tilt < self.ROTATION:
                self.tilt = self.ROTATION
        else:
            if self.tilt < -90:
                self.tilt -= self.ROTATION_VELOCITY

    def draw(self, win):
        self.imgCount += 1

        if self.imgCount < self.ANIMATION_TIME:
            self.img = self.IMGS[0]
        elif self.imgCount < self.ANIMATION_TIME *10:
            self.img = self.IMGS[1]
        elif self.imgCount < self.ANIMATION_TIME *20:
            self.img = self.IMGS[2]
        elif self.imgCount < self.ANIMATION_TIME *30:
            self.img = self.IMGS[1]
        elif self.imgCount < self.ANIMATION_TIME *40:
            self.img = self.IMGS[0]
            self.imgCount = 0


        if self.tilt <= -80:
            self.img = self.IMGS[1]
            self.imgCount = self.ANIMATION_TIME*2


        rotated = pygame.transform.rotate(self.img, self.tilt)
        newRect = rotated.get_rect(center = self.img.get_rect(topleft = (self.x, self.y)).center)
        win.blit(rotated, newRect.topleft)

    def getMask(self):
        return pygame.mask.from_surface(self.img)

class Pipe():
    GAP = 200
    VELOCITY = 5

    def __init__(self, x):
        self.x = x
        self.height = 0
        self.gap = 150

        self.top = 0
        self.bottom = 0
        self.PIPE_TOP = pygame.transform.flip(PIPE_IMAGE, False, True)
        self.PIPE_BOTTOM = PIPE_IMAGE

        self.passed = False
        self.setHeight()
    
    def setHeight(self):
        self.height = random.randrange(0,350)
        self.top = self.height - self.PIPE_TOP.get_height()
        self.bottom = self.height + self.gap

    def move(self):
        self.x -= self.VELOCITY
    
    def draw(self, win):
        win.blit(self.PIPE_TOP, (self.x, self.top))
        win.blit(self.PIPE_BOTTOM, (self.x, self.bottom))

    def collide(self, bun):
        #creating masks and seeing if they collide
        bunMask = bun.getMask()
        topMask = pygame.mask.from_surface(self.PIPE_TOP)
        bottomMask = pygame.mask.from_surface(self.PIPE_BOTTOM)

        topOffset = (self.x - bun.x, self.top - round(bun.y))
        bottomOffset  = (self.x - bun.x, self.bottom - round(bun.y))

        bPoint = bunMask.overlap(bottomMask, bottomOffset) #returns none if doesn't collide
        tPoint = bunMask.overlap(topMask, topOffset)

        if tPoint or bPoint:
            return True
        return False

class Base:
    VELOCITY = 5
    WIDTH = BASE_IMAGE.get_width()
    IMG = BASE_IMAGE

    def __init__(self, y):
        self.y = y
        self.x1 = 0
        self.x2 = self.WIDTH

    def move(self):
        #basically there are two images following each other illusion
        self.x1 -= self.VELOCITY
        self.x2 -= self.VELOCITY

        if self.x1 + self.WIDTH < 0:
            self.x1 = self.x2 + self.WIDTH

        if self.x2 + self.WIDTH < 0:
            self.x2 = self.x1 + self.WIDTH

    def draw(self, win):
        win.blit(self.IMG, (self.x1, self.y))
        win.blit(self.IMG, (self.x2, self.y))

def drawWindow(win, buns, pipes, base, score):
    win.blit(BG_IMAGE, (0,0))
    for pipe in pipes:
        pipe.draw(win)

    text = STAT_FONT.render("Score: " + str(score), 1, (255,255,255))
    win.blit(text, (WIDTH - 10 - text.get_width(), 10))

    #base.draw(win)    
    for bun in buns:
        bun.draw(win)
    pygame.display.update()

#main is the fitness function for the neat algorithm
def eval_genomes(genomes, config):
    nets = []
    ge = []
    bunnies = []

    for genomeID, g in genomes:
        g.fitness = 0
        net = neat.nn.FeedForwardNetwork.create(g, config)
        nets.append(net)
        bunnies.append(Bun(200, 250))
        ge.append(g)

    base = Base(430)
    pipes = [Pipe(250)]

    run = True
    win = pygame.display.set_mode((WIDTH, HEIGHT))
    clock = pygame.time.Clock()
    score = 0

    while run and len(bunnies) > 0:
        clock.tick(30)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                run = False
                pygame.quit()
                quit()

        pipeIndex = 0
        if len(bunnies) > 0:
            if len(pipes) > 1 and bunnies[0].x > pipes[0].x + pipes[0].PIPE_TOP.get_width():
                pipeIndex = 1

        for x, bunny in enumerate(bunnies):
            bunny.move()
            ge[x].fitness += 0.1

            output = nets[x].activate((bunny.y, abs(bunny.y - pipes[pipeIndex].height), abs(bunny.y - pipes[pipeIndex].bottom)))
            if output[0] > 0.5:
                bunny.jump()

        rem = []
        addPipe = False
        for pipe in pipes:
            pipe.move()
            for x, bunny in enumerate(bunnies):
                if pipe.collide(bunny):
                    ge[x].fitness -= 1
                    bunnies.pop(x)
                    nets.pop(x)
                    ge.pop(x)

                if pipe.x + pipe.PIPE_TOP.get_width() < 0:
                    rem.append(pipe)
     
                if not pipe.passed and pipe.x < bunny.x:
                    pipe.passed = True
                    addPipe = True

        if addPipe:
            score += 1
            for g in ge:
                g.fitness += 5
            pipes.append(Pipe(460))

        for r in rem:
            if r in pipes:
                pipes.remove(r)

        for x, bunny in enumerate(bunnies):
            if bunny.y + bunny.img.get_height() >= HEIGHT or bunny.y < 0:
                bunnies.pop(x)
                nets.pop(x)
                ge.pop(x)

        drawWindow(win, bunnies, pipes, base, score)

    # Evaluate the fitness of the genomes using the NEAT evaluate function
    for x, g in enumerate(ge):
        g.fitness += 0.1 * bunnies[x].tick  # Adjust fitness based on ticks survived

def run(configPath):
    config = neat.config.Config(neat.DefaultGenome, neat.DefaultReproduction,
                                neat.DefaultSpeciesSet, neat.DefaultStagnation,
                                configPath)

    p = neat.Population(config)
    p.add_reporter(neat.StdOutReporter(True))
    stats = neat.StatisticsReporter()
    p.add_reporter(stats)

    # Run the NEAT algorithm with the evaluate_genomes function
    winner = p.run(eval_genomes, 50)

if __name__ == "__main__":
    localDir = os.path.dirname(__file__)
    configPath = os.path.join(localDir, "config.txt")
    run(configPath)





if __name__ == "__main__":
    localDir = os.path.dirname(__file__)
    configPath = os.path.join(localDir, "config.txt")
    run(configPath)
