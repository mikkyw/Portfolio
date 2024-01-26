import pygame
import sys
import random

# Initialize Pygame
pygame.init()

# Constants
WIDTH, HEIGHT = 600, 400
PIPE_WIDTH = 50
PIPE_HEIGHT = random.randint(100, 300)
PIPE_SPEED = 5
BIRD_SIZE = 30
GRAVITY = 1
JUMP_SPEED = 8
MAX_SPEED = 10

# Colors
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)

# Create the screen
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Flappy Bird")

background_image = pygame.image.load('space.jpeg')  
background_surface = pygame.transform.scale(background_image, (WIDTH, HEIGHT))

# Create the bird
bird = pygame.Rect(WIDTH // 4, HEIGHT // 2, BIRD_SIZE, BIRD_SIZE)
birdVelocity = 0

# Create the pipes
pipes = []
pipeHeights = [random.randint(100, 300) for _ in range(3)]
pipe_gap = 100  # Adjust the gap between the top and bottom pipes

for i in range(3):
    pipe_height = random.randint(100, 300)
    top_pipe = pygame.Rect(WIDTH + i * 200, 0, PIPE_WIDTH, pipe_height)
    bottom_pipe = pygame.Rect(WIDTH + i * 200, pipe_height + pipe_gap, PIPE_WIDTH, HEIGHT - pipe_height - pipe_gap)

    pipes.extend([top_pipe, bottom_pipe])

clock = pygame.time.Clock()

while True:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            sys.exit()
        if event.type == pygame.KEYDOWN:
            if event.key == pygame.K_SPACE: 
                birdVelocity = -JUMP_SPEED

    screen.blit(background_surface, (0, 0))
    
    # Update bird position
    birdVelocity += GRAVITY
    birdVelocity = min(birdVelocity, MAX_SPEED)

    bird.y += birdVelocity

    # Update pipe positions
    for pipe in pipes:
        pipe.x -= PIPE_SPEED
        if pipe.right < 0:
            # Reset pipe position when it goes off-screen
            pipe.x = WIDTH
            if pipe.height == pipeHeights[0]:
                pipe.height = pipeHeights[1]
            elif pipe.height == pipeHeights[1]:
                pipe.height = pipeHeights[2]
            elif pipe.height == pipeHeights[2]:
                pipe.height = random.randint(100, 300)

    #pipe collision
    for pipe in pipes:
        if bird.colliderect(pipe):
            pygame.quit()
            sys.exit()


    pygame.draw.rect(screen, WHITE, bird)

    for pipe in pipes:
        pygame.draw.rect(screen, WHITE, pipe)

    pygame.display.flip()

    clock.tick(30)
 
